## AutoCAD to Base44 bridge

This repository also contains a small AutoCAD .NET plug-in that sends selected drawing-object metadata to a Base44 HTTP endpoint. It is intended for a Base44 app that tracks drawings, rooms, measurements, and estimates. It is intentionally endpoint-agnostic so it can work with a Base44 webhook, API route, or an integration endpoint exposed by another plug-in.

## Commands

- `NETLOAD` — load `AutoCadBase44Bridge.dll` in AutoCAD.
- `BASE44CONFIG` — save the Base44 webhook URL and optional bearer API key for the current Windows user.
- `BASE44SEND` — select objects and send their handles, entity types, layers, drawing name, drawing units, and bounding-box coordinates as JSON.

The plug-in does not infer room names, prices, or quantities from arbitrary geometry. That keeps imported measurements accurate. The Base44 app should map imported objects to rooms and apply its estimate rules explicitly.

The payload uses this shape:

```json
{
  "eventType": "autocad.selection.created",
  "drawingName": "C:\\\\drawings\\\\site.dwg",
  "drawingUnits": "Millimeters",
  "sentAtUtc": "2026-01-01T00:00:00Z",
  "objects": [
    {
      "handle": "2A1",
      "entityType": "Line",
      "layer": "Walls",
      "minPoint": { "x": 0, "y": 0, "z": 0 },
      "maxPoint": { "x": 10, "y": 5, "z": 0 }
    }
  ]
}
```

## Build

Install the .NET Framework 4.8 developer pack and the full AutoCAD version you target. AutoCAD LT does not expose the complete .NET plug-in API required by this project. Set `AUTOCAD_INSTALL_DIR` to the directory containing `AcMgd.dll` and `AcDbMgd.dll`, then build:

```powershell
$env:AUTOCAD_INSTALL_DIR = "C:\Program Files\Autodesk\AutoCAD 2025"
dotnet build .\AutoCadBase44Bridge\AutoCadBase44Bridge.csproj
```

AutoCAD's managed API assemblies are installed with AutoCAD and are not redistributed by this project.

## Base44 setup

Create the Base44 backend function `importDrawingPayload` and copy its deployed endpoint URL into `BASE44CONFIG`. Add a long random value named `AUTOCAD_BRIDGE_API_KEY` under Base44 Settings -> Environment Variables. The function must compare the incoming `Authorization: Bearer <token>` value with that secret before parsing or storing the payload. Enter the same key when `BASE44CONFIG` prompts for the API key; the plug-in stores it in the current user's Windows registry and sends it only over HTTPS.

The OpenAPI importer can use the raw specification at:
`https://raw.githubusercontent.com/jgelchaar/pets-workshop/jgelchaar-autocad-integration/openapi.yaml`
After importing it, replace `YOUR_BASE44_FUNCTION_HOST` with the host shown for your Base44 `importDrawingPayload` function.

The endpoint contract is:

- Method: `POST`
- Header: `Authorization: Bearer <AUTOCAD_BRIDGE_API_KEY>`
- Header: `Content-Type: application/json`
- Body: the `DrawingPayload` JSON shown above
- Success: any `2xx` response
- Authentication failure: `401`
- Invalid payload: `400`

Do not commit the key, put it in the repository, or paste it into source files. The receiving function should validate required fields (`eventType`, `drawingName`, `drawingUnits`, and every object handle), enforce a reasonable object-count/body-size limit, and upsert objects by drawing plus AutoCAD handle.

## Planned companion features

The Base44 specification also covers a review-first drawing assistant, live surrounding-area context layers, and PDF conversion jobs. The assistant should emit explicit vector operations and require approval before modifying a drawing. Map data should remain a separate context layer, with OpenStreetMap attribution or user-supplied Google credentials. For PDF-to-DXF work, prefer AutoCAD's native `PDFIMPORT`; scanned PDFs cannot be treated as accurate vectors without verification.

## Pets workshop

This repository contains the project for three guided workshops to explore various GitHub features. The project is a website for a fictional dog shelter, with a [Flask](https://flask.palletsprojects.com/en/stable/) backend using [SQLAlchemy](https://www.sqlalchemy.org/) and an [Astro](https://astro.build/) frontend using [Tailwind CSS](https://tailwindcss.com/).

The available workshops are:

- **[One hour](./content/1-hour/README.md)** — focused on GitHub Copilot
- **[Full-day](./content/full-day/README.md)** — a full day-in-the-life of a developer using GitHub for their DevOps processes
- **[GitHub Actions](./content/github-actions/README.md)** — CI/CD pipelines from running tests to deploying to Azure

## Getting started

> **[Get started learning about development with GitHub!](./content/README.md)**

## License 

This project is licensed under the terms of the MIT open source license. Please refer to the [LICENSE](./LICENSE) for the full terms.

## Maintainers 

You can find the list of maintainers in [CODEOWNERS](./.github/CODEOWNERS).

## Support

This project is provided as-is, and may be updated over time. If you have questions, please open an issue.
