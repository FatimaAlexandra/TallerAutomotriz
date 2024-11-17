using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using amazon.Models;
using TuNamespace.Models;

namespace amazon.Services
{
    public class PdfService
    {
        public byte[] GenerateInvoicePdf(Facturacion factura, bool isReceipt = false)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(container => ComposeHeader(container, factura.Usuario.Nombre, isReceipt));
                    page.Content().Element(container => ComposeContent(container, factura, isReceipt));
                    page.Footer().Element(container => ComposeFooter(container, isReceipt));
                });
            }).GeneratePdf();
        }

        private void ComposeHeader(IContainer container, string clienteName, bool isReceipt)
        {
            container.Row(row =>
            {
                row.ConstantItem(100).Image("wwwroot/images/logo.png");
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text(isReceipt ? "RECIBO" : "FACTURA")
                        .FontSize(20)
                        .Bold();
                    column.Item().Text("Marland Auto Servicio")
                        .FontSize(16)
                        .Bold();
                    column.Item().Text("Dirección: Calle Principal #123, San Salvador")
                        .FontSize(10);
                    column.Item().Text("Tel: (503) 2222-2222")
                        .FontSize(10);
                    column.Item().Text("Email: marland@example.com")
                        .FontSize(10);
                });
                row.ConstantItem(100).Column(column =>
                {
                    column.Item().Text($"Para: {clienteName}")
                        .FontSize(14)
                        .Bold();
                });
            });
        }

        private void ComposeContent(IContainer container, Facturacion factura, bool isReceipt)
        {
            container.PaddingVertical(20).Column(column =>
            {
                // Información de factura
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(x =>
                    {
                        x.Item().Text($"{(isReceipt ? "RECIBO" : "FACTURA")} N°: {factura.Id}")
                            .Bold()
                            .FontSize(16);
                        x.Item().Text($"Fecha: {factura.FechaFacturacion:dd/MM/yyyy}");
                        x.Item().Text($"Estado: {factura.EstadoFactura}");
                        x.Item().Text($"Método de Pago: {factura.MetodoPago}");
                    });
                });

                // Tabla de servicios
                column.Item().PaddingTop(20).Table(table =>
                {
                    // Definición de columnas
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);    // Servicio
                        columns.RelativeColumn();     // Vehículo
                        columns.RelativeColumn();     // Cantidad
                        columns.RelativeColumn();     // Precio Unit.
                        columns.RelativeColumn();     // Subtotal
                    });

                    // Encabezados
                    table.Header(header =>
                    {
                        header.Cell().Background("#257272").Padding(2).Text("Servicio").Bold().FontColor("#fff");
                        header.Cell().Background("#257272").Padding(2).Text("Vehículo").Bold().FontColor("#fff");
                        header.Cell().Background("#257272").Padding(2).AlignRight().Text("Cantidad").Bold().FontColor("#fff");
                        header.Cell().Background("#257272").Padding(2).AlignRight().Text("Precio Unit.").Bold().FontColor("#fff");
                        header.Cell().Background("#257272").Padding(2).AlignRight().Text("Subtotal").Bold().FontColor("#fff");
                    });

                    // Detalles
                    foreach (var detalle in factura.DetalleFacturacion)
                    {
                        var servicio = detalle.ServicioRealizado?.Servicio;
                        var vehiculo = detalle.ServicioRealizado?.Vehiculo;

                        table.Cell().BorderBottom(1).BorderColor("#D1D1D1").Padding(2)
                            .Text(servicio?.Nombre ?? "");

                        table.Cell().BorderBottom(1).BorderColor("#D1D1D1").Padding(2)
                            .Text($"{vehiculo?.Marca} {vehiculo?.Modelo}\n{vehiculo?.Placa}");

                        table.Cell().BorderBottom(1).BorderColor("#D1D1D1").Padding(2).AlignRight()
                            .Text("1");

                        table.Cell().BorderBottom(1).BorderColor("#D1D1D1").Padding(2).AlignRight()
                            .Text($"{detalle.PrecioUnitario:C2}");

                        table.Cell().BorderBottom(1).BorderColor("#D1D1D1").Padding(2).AlignRight()
                            .Text($"{detalle.Subtotal:C2}");
                    }
                });

                // Total
                column.Item().PaddingTop(10).AlignRight()
                    .Text($"Total: {factura.MontoTotal:C2}")
                    .Bold()
                    .FontSize(16);

                // Términos y condiciones
                column.Item().PaddingTop(25).Column(x =>
                {
                    x.Item().Text("Términos y Condiciones").Bold().FontSize(12);
                    x.Item().Text("1. Los precios incluyen IVA");
                    x.Item().Text("2. Garantía de servicios: 30 días");
                    x.Item().Text("3. No se aceptan devoluciones después de 15 días");
                    x.Item().Text("4. Cualquier reclamo o consulta, favor comunicarse al (503) 2222-2222");
                });
            });
        }

        private void ComposeFooter(IContainer container, bool isReceipt)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text(isReceipt ? "¡Gracias por su preferencia!" : "Gracias por su preferencia")
                        .FontSize(12)
                        .Bold()
                        .FontColor("#257272");

                    column.Item().PaddingTop(5).Text(text =>
                    {
                        text.Span("Página ");
                        text.CurrentPageNumber();
                        text.Span(" de ");
                        text.TotalPages();
                    });

                    column.Item().PaddingTop(5).Text("© 2024 MARLAND AUTO SERVICIO")
                        .FontSize(8);
                });
            });
        }

        public string GetInvoiceFileName(Facturacion factura, bool isReceipt = false)
        {
            return $"{(isReceipt ? "Recibo" : "Factura")}-{factura.Usuario.Nombre.Replace(" ", "_")}-{factura.Id}.pdf";
        }
    }
}