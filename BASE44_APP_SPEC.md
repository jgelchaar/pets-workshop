# Base44 app implementation specification

Use this specification in the Base44 builder for the companion app.

## Purpose

Track AutoCAD drawings, rooms, measured geometry, estimate line items, vector drawing requests, map context layers, and PDF conversion jobs without changing imported measurements.

## Entities

### Drawing

- `name` — text
- `sourcePath` — text
- `units` — text
- `lastImportedAt` — datetime
- `importedObjectCount` — number

### Room

- `drawingId` — relation to Drawing
- `name` — text
- `level` — text
- `notes` — long text

### DrawingObject

- `drawingId` — relation to Drawing
- `handle` — text
- `entityType` — text
- `layer` — text
- `minX`, `minY`, `minZ` — number
- `maxX`, `maxY`, `maxZ` — number
- `roomId` — optional relation to Room

### EstimateLine

- `roomId` — optional relation to Room
- `description` — text
- `quantity` — number
- `unit` — text
- `unitPrice` — number
- `total` — calculated as `quantity * unitPrice`
- `notes` — long text

### DrawingRequest

- `drawingId` — relation to Drawing
- `prompt` — long text
- `status` — enum: pending, approved, processing, complete, failed
- `requestedBy` — user
- `resultSummary` — long text

### ContextLayer

- `drawingId` — relation to Drawing
- `provider` — enum: openstreetmap, google
- `layerType` — enum: roads, buildings, parcels, satellite, terrain
- `sourceUrl` — text
- `capturedAt` — datetime
- `geometry` — JSON or GeoJSON
- `licenseNotice` — long text

### PdfConversionJob

- `drawingId` — optional relation to Drawing
- `sourceFile` — file
- `outputFile` — file
- `status` — enum: queued, processing, complete, failed
- `conversionMethod` — enum: autocad_pdfimport, external_converter
- `pageNumber` — number
- `warnings` — long text

## Import behavior

Create a POST endpoint or workflow that accepts the `DrawingPayload` from the AutoCAD plug-in:

1. Find or create the Drawing by `sourcePath`/`name`.
2. Update its units and import timestamp.
3. Upsert DrawingObject records by `(drawingId, handle)`.
4. Never convert coordinates unless the drawing unit is explicitly known.
5. Do not create rooms or estimate prices automatically from arbitrary entities.
6. Show an import result with created, updated, skipped, and failed counts.

## Natural-language vector drawing

Add a Drawing Assistant that turns a user request into a reviewable vector plan, not an immediately committed drawing. A request such as “draw a 12 by 10 foot room with a 3 foot door on the east wall” should produce:

- normalized units and dimensions
- explicit line, polyline, arc, circle, block, and text operations
- a preview and validation warnings
- an approval step before sending operations to AutoCAD

Never invent dimensions, coordinates, parcel boundaries, or prices. Ask for missing dimensions or mark assumptions visibly.

## Maps and surrounding context

Support two provider adapters:

- OpenStreetMap/Overpass for roads and building context, with attribution stored in `licenseNotice`.
- Google Maps/Google Earth APIs for users who provide their own enabled API credentials and accept Google's current terms and display requirements.

Store imported map geometry as a context layer and keep it separate from measured design geometry. Reproject map coordinates into the drawing coordinate system only after the user confirms the drawing coordinate reference system, origin, and units.

## PDF to DXF

Prefer AutoCAD's native `PDFIMPORT` for vector PDFs because it preserves AutoCAD-native geometry and lets the user choose page, scale, layers, and cleanup options. Treat scanned PDFs as raster inputs requiring OCR/vectorization and manual verification. An external converter may be offered as an explicit fallback, but the app must show its warnings and never represent an unverified conversion as accurate.

## Accuracy rules

- Preserve the AutoCAD unit label and raw coordinate values.
- Use decimal numbers, not formatted strings, for coordinates, quantities, and prices.
- Require a user to confirm room assignment and estimate pricing.
- Keep an import history record containing timestamp, drawing name, and object count.
- Reject payloads with missing `eventType`, `drawingName`, `drawingUnits`, or object handles.

## Suggested Base44 builder prompt

> Build a responsive app for AutoCAD drawing takeoffs. Add Drawing, Room, DrawingObject, EstimateLine, and ImportHistory entities using the fields in `BASE44_APP_SPEC.md`. Add a secure JSON POST integration endpoint for the AutoCAD selection payload. Upsert drawings by source path and drawing objects by drawing plus AutoCAD handle. Preserve raw coordinates and units exactly. Provide pages for drawings, rooms, imported geometry, and estimates. Show import results and validation errors. Do not infer room names or prices from geometry; require explicit user confirmation. Add role-based access, audit history, and test data for a two-room drawing.

> Extend the app with DrawingRequest, ContextLayer, and PdfConversionJob. Add a Drawing Assistant that converts natural-language requests into reviewable vector operations and requires approval before AutoCAD changes. Add OpenStreetMap/Overpass and optional user-configured Google map adapters with attribution, API-key security, coordinate-system confirmation, and separate context layers. Add PDF conversion workflow: prefer native AutoCAD PDFIMPORT for vector PDFs, label scanned PDFs as requiring verification, and allow an explicitly configured external converter fallback with warnings.
