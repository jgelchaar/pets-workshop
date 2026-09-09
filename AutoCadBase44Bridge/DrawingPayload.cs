using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.ApplicationServices;

namespace AutoCadBase44Bridge;

internal sealed record DrawingPayload(
    string EventType,
    string DrawingName,
    string DrawingUnits,
    DateTime SentAtUtc,
    IReadOnlyList<DrawingObjectPayload> Objects)
{
    public static DrawingPayload Create(Document document, SelectionSet selection)
    {
        var database = document.Database;
        var objects = new List<DrawingObjectPayload>();

        using var transaction = database.TransactionManager.StartTransaction();
        foreach (var objectId in selection.GetObjectIds())
        {
            if (transaction.GetObject(objectId, OpenMode.ForRead) is not Entity entity)
            {
                continue;
            }

            var bounds = entity.GeometricExtents;
            objects.Add(new DrawingObjectPayload(
                entity.Handle.ToString(),
                entity.GetType().Name,
                entity.Layer,
                new PointPayload(bounds.MinPoint),
                new PointPayload(bounds.MaxPoint)));
        }

        transaction.Commit();
        return new DrawingPayload(
            "autocad.selection.created",
            document.Name,
            database.Insunits.ToString(),
            DateTime.UtcNow,
            objects);
    }
}

internal sealed record DrawingObjectPayload(
    string Handle,
    string EntityType,
    string Layer,
    PointPayload MinPoint,
    PointPayload MaxPoint);

internal sealed record PointPayload(double X, double Y, double Z)
{
    public PointPayload(Point3d point) : this(point.X, point.Y, point.Z)
    {
    }
}
