#!/usr/bin/env python3
"""Small dependency-free MCP stdio bridge for the API Tester HTTP API."""

import json
import os
import sys
import urllib.error
import urllib.parse
import urllib.request

BASE_URL = os.environ.get("API_TESTER_URL", "http://localhost:5000").rstrip("/")
COOKIE = os.environ.get("API_TESTER_COOKIE", "")

TOOLS = [
    {
        "name": "listWorkspaces",
        "description": "List API Tester workspaces.",
        "inputSchema": {
            "type": "object",
            "properties": {"query": {"type": "string"}},
            "additionalProperties": False,
        },
    },
    {
        "name": "listCollections",
        "description": "List API Tester collections.",
        "inputSchema": {
            "type": "object",
            "properties": {"query": {"type": "string"}},
            "additionalProperties": False,
        },
    },
    {
        "name": "listRequests",
        "description": "List saved API requests.",
        "inputSchema": {
            "type": "object",
            "properties": {"query": {"type": "string"}},
            "additionalProperties": False,
        },
    },
    {
        "name": "getRequest",
        "description": "Get one saved request by numeric ID.",
        "inputSchema": {
            "type": "object",
            "properties": {"id": {"type": "integer", "minimum": 1}},
            "required": ["id"],
            "additionalProperties": False,
        },
    },
    {
        "name": "createRequest",
        "description": "Create a saved request. Requires an Admin or Manager session cookie.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {"type": "string"},
                "url": {"type": "string"},
                "method": {"type": "integer", "description": "HttpMethodType enum: Get=0, Post=1, Put=2, Delete=3, Patch=4."},
                "body": {"type": "string"},
                "collectionId": {"type": "integer", "minimum": 1},
                "environmentId": {"type": ["integer", "null"]},
                "tagIds": {"type": ["array", "null"], "items": {"type": "integer"}},
            },
            "required": ["name", "url", "method", "collectionId"],
            "additionalProperties": False,
        },
    },
]


def api_request(method, path, payload=None):
    headers = {"Accept": "application/json"}
    if COOKIE:
        headers["Cookie"] = COOKIE
    data = None
    if payload is not None:
        data = json.dumps(payload).encode("utf-8")
        headers["Content-Type"] = "application/json"
    request = urllib.request.Request(BASE_URL + path, data=data, headers=headers, method=method)
    try:
        with urllib.request.urlopen(request, timeout=30) as response:
            body = response.read().decode("utf-8")
            return json.loads(body) if body else {"status": response.status}
    except urllib.error.HTTPError as error:
        body = error.read().decode("utf-8", errors="replace")
        raise RuntimeError(f"API returned HTTP {error.code}: {body}") from error
    except urllib.error.URLError as error:
        raise RuntimeError(f"Could not connect to API Tester at {BASE_URL}: {error.reason}") from error


def call_tool(name, arguments):
    if name == "listWorkspaces":
        return api_request("GET", "/api/workspaces?" + urllib.parse.urlencode({"q": arguments.get("query", "")}))
    if name == "listCollections":
        return api_request("GET", "/api/collections?" + urllib.parse.urlencode({"q": arguments.get("query", "")}))
    if name == "listRequests":
        return api_request("GET", "/api/requests?" + urllib.parse.urlencode({"q": arguments.get("query", "")}))
    if name == "getRequest":
        return api_request("GET", f"/api/requests/{int(arguments['id'])}")
    if name == "createRequest":
        return api_request("POST", "/api/requests", arguments)
    raise RuntimeError(f"Unknown tool: {name}")


def send(payload):
    sys.stdout.write(json.dumps(payload, separators=(",", ":")) + "\n")
    sys.stdout.flush()


def handle(message):
    request_id = message.get("id")
    method = message.get("method")
    if method == "initialize":
        return {
            "jsonrpc": "2.0",
            "id": request_id,
            "result": {
                "protocolVersion": message.get("params", {}).get("protocolVersion", "2025-11-25"),
                "capabilities": {"tools": {}},
                "serverInfo": {"name": "api-tester", "version": "1.0.0"},
            },
        }
    if method == "tools/list":
        return {"jsonrpc": "2.0", "id": request_id, "result": {"tools": TOOLS}}
    if method == "tools/call":
        params = message.get("params", {})
        try:
            result = call_tool(params.get("name", ""), params.get("arguments", {}))
            return {
                "jsonrpc": "2.0",
                "id": request_id,
                "result": {
                    "content": [{"type": "text", "text": json.dumps(result, indent=2)}],
                    "structuredContent": {"result": result},
                },
            }
        except Exception as error:
            return {
                "jsonrpc": "2.0",
                "id": request_id,
                "result": {"content": [{"type": "text", "text": str(error)}], "isError": True},
            }
    if request_id is not None:
        return {
            "jsonrpc": "2.0",
            "id": request_id,
            "error": {"code": -32601, "message": f"Method not found: {method}"},
        }
    return None


for line in sys.stdin:
    if not line.strip():
        continue
    try:
        response = handle(json.loads(line))
        if response is not None:
            send(response)
    except Exception as error:
        send({"jsonrpc": "2.0", "id": None, "error": {"code": -32700, "message": str(error)}})
