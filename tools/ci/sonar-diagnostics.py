"""Read-only PR diagnostics using the existing CI token; never print raw responses."""
import json
import os
import sys
from urllib.error import HTTPError, URLError
from urllib.parse import urlencode
from urllib.request import Request, urlopen


def snapshot(token, pull_request, get=None):
    def read(endpoint, params):
        request = Request("https://sonarcloud.io/api/" + endpoint + "?" + urlencode(params),
                          headers={"Authorization": "Bearer " + token})
        with urlopen(request, timeout=20) as response:
            return json.load(response)

    get = get or read
    project = "frankcval_MiKompri"
    status = get("qualitygates/project_status", {"projectKey": project, "pullRequest": pull_request})
    issues = []
    for page in range(1, 6):
        result = get("issues/search", {"componentKeys": project, "pullRequest": pull_request,
                     "sinceLeakPeriod": "true", "resolved": "false", "ps": 100, "p": page})
        for issue in result.get("issues", []):
            impacts = issue.get("impacts", [])
            if issue.get("type") == "VULNERABILITY" or any(i.get("softwareQuality") == "SECURITY" for i in impacts):
                issues.append({key: issue.get(key) for key in ("key", "rule", "component", "line", "type", "severity", "impacts")})
        if page * 100 >= result.get("total", result.get("paging", {}).get("total", 0)):
            break
    measures = get("measures/component_tree", {"component": project, "pullRequest": pull_request,
                   "metricKeys": "new_coverage,new_lines_to_cover,new_uncovered_lines", "qualifiers": "FIL", "ps": 500})
    gate = status.get("projectStatus", {})
    return {"pullRequest": pull_request, "gate": {"status": gate.get("status"),
            "conditions": [{key: c.get(key) for key in ("status", "metricKey", "comparator", "errorThreshold", "actualValue")}
                           for c in gate.get("conditions", [])]}, "securityIssues": issues,
            "issuesTruncated": result.get("total", result.get("paging", {}).get("total", 0)) > 500,
            "coverageTruncated": measures.get("paging", {}).get("total", 0) > 500,
            "coverageFiles": [{"path": c.get("path"), "measures": c.get("measures", [])}
                              for c in measures.get("components", []) if c.get("measures")]}


if __name__ == "__main__":
    try:
        token = os.environ["SONAR_TOKEN"]
        number = int(os.environ["SONAR_PR_NUMBER"])
        if not token or number < 1:
            raise ValueError()
        print(json.dumps(snapshot(token, number), indent=2))
    except HTTPError as error:
        print("Sonar diagnostics unavailable: HTTP " + str(error.code), file=sys.stderr)
        sys.exit(1)
    except (KeyError, ValueError, URLError):
        print("Sonar diagnostics unavailable: check CI configuration or connectivity.", file=sys.stderr)
        sys.exit(1)
