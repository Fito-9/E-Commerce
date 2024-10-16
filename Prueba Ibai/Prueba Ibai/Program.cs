using System.Diagnostics;
using Microsoft.Playwright;
namespace PruebaPlaywright;
public class Program
{
    public static async Task Main()
    {
        // Necesario para instalar los navegadores
        Microsoft.Playwright.Program.Main(["install"]);
        using IPlaywright playwright = await Playwright.CreateAsync();
        BrowserTypeLaunchOptions options = new BrowserTypeLaunchOptions()
        {
            Headless = false // Se indica falso para poder ver el navegador
        };
        await using IBrowser browser = await
        playwright.Chromium.LaunchAsync(options);
        await using IBrowserContext context = await browser.NewContextAsync();
        IPage page = await context.NewPageAsync();
        // Ir a la página de cartooncorp
        await page.GotoAsync("https://cartooncorp.es");
        // Escribimos en la barra de búsqueda lo que queremos buscar
        IElementHandle searchInput = await page.QuerySelectorAsync("#search_widget > form > input");
        await searchInput.FillAsync("sobre pokemon");
        // Le damos al botón de buscar
        IElementHandle searchButton = await page.QuerySelectorAsync("#search_widget > form > button");
        await searchButton.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        // Recorremos la lista de productos y recolectamos los datos
        List<Product> products = new List<Product>();
        IReadOnlyList<IElementHandle> productElements = await
        page.QuerySelectorAllAsync(".product-description");

        for (int i = 0; i <= 9; i++)
        {
            try
            {
                Product product = await GetProductAsync(productElements[i]);
                products.Add(product);
                Console.WriteLine(product);

            }
            catch { }
        }
        Product cheapest = products.MinBy(p => p.Price);
        Product expensive = products.MaxBy(p => p.Price);
        decimal average = products.Average(p => p.Price);

        Console.WriteLine($"La oferta más barata es: {cheapest.Price}");
        Console.WriteLine($"La oferta más cara es: {expensive.Price}");
        Console.WriteLine($"La media de los precios de los productos es: {average}");
    }
    private static async Task<Product> GetProductAsync(IElementHandle element)
    {
        IElementHandle priceElement = await element.QuerySelectorAsync("span.price");
        string priceRaw = await priceElement.InnerTextAsync();

        // Eliminar caracteres especiales, espacios no separables y el símbolo de la moneda
        priceRaw = System.Text.RegularExpressions.Regex.Replace(priceRaw, @"[^\d,\.]", "");

        // Reemplazar la coma por punto para el formato decimal
        priceRaw = priceRaw.Replace(",", ".").Trim();

        // Parsear el precio
        decimal price;
        if (!decimal.TryParse(priceRaw, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out price))
        {
            throw new FormatException($"Error al convertir el precio: {priceRaw}");
        }

        string name = ""; // Nombre del producto
        

        return new Product(price);
    }


}



