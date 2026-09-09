# AutoCAD to Base44 bridge

This repository contains a small AutoCAD .NET plug-in that sends selected drawing-object metadata to a Base44 HTTP endpoint. It is intended for a Base44 app that tracks drawings, rooms, measurements, and estimates. It is intentionally endpoint-agnostic so it can work with a Base44 webhook, API route, or an integration endpoint exposed by another plug-in.

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

Create a Base44 endpoint that accepts a JSON POST and copy its URL into `BASE44CONFIG`. If the endpoint requires authentication, enter its API key when prompted. The receiving endpoint should validate the bearer token and treat drawing data as untrusted input.

## Planned companion features

The Base44 specification also covers a review-first drawing assistant, live surrounding-area context layers, and PDF conversion jobs. The assistant should emit explicit vector operations and require approval before modifying a drawing. Map data should remain a separate context layer, with OpenStreetMap attribution or user-supplied Google credentials. For PDF-to-DXF work, prefer AutoCAD's native `PDFIMPORT`; scanned PDFs cannot be treated as accurate vectors without verification.
