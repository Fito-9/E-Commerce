using System.Diagnostics;
using System.Xml;
using Microsoft.Playwright;
using static System.Net.Mime.MediaTypeNames;
namespace PruebaPlaywright;
using PruebaPlaywright;
public class Program
{
    static async Task Main()
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

        // Ir a la página de ebay
        await page.GotoAsync("https://cartooncorp.es");

        // IElementHandle buscar = await page.QuerySelectorAsync("#menu-1-c9878bc > li.astm-search-menu.is-menu.is-dropdown.menu-item > a > svg > path");
        //await buscar.ClickAsync();
        // Escribimos en la barra de búsqueda lo que queremos buscar
        IElementHandle searchInput = await page.QuerySelectorAsync("#search_widget > form > input");

        await searchInput.FillAsync("sobre pokemon");
        // Le damos al botón de buscar
        IElementHandle searchButton = await page.QuerySelectorAsync("#search_widget > form > button");
        await searchButton.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // Recorremos la lista de productos y recolectamos los datos
        List<Product> products = new List<Product>();
        IReadOnlyList<IElementHandle> productElements = await page.QuerySelectorAllAsync("#js-product-list > div.products.row");

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
       
        if (products.Count > 0)
        {
            Product cheapest = products.MinBy(p => p.Price);
            Product expensive = products.MaxBy(p => p.Price);
            decimal average = products.Average(p => p.Price);

            Console.WriteLine($"La oferta más barata es: {cheapest}");
            Console.WriteLine($"La oferta más cara es: {expensive}");
            Console.WriteLine($"La media de los precios de los productos es: {average}");
        }
        else
        {
            Console.WriteLine("No se encontraron productos.");
        }

        /*
        ProcessStartInfo processInfo = new ProcessStartInfo()
        {
            FileName = cheapest.Url,
            UseShellExecute = true
        };
        Process.Start(processInfo);
        */
    }
    private static async Task<Product> GetProductAsync(IElementHandle element)
    {
        IElementHandle priceElement = await element.QuerySelectorAsync("span.price");
        string priceRaw = await priceElement.InnerTextAsync();
        priceRaw = priceRaw.Replace(" €", "", StringComparison.OrdinalIgnoreCase);
        priceRaw = priceRaw.Trim();
        decimal price = decimal.Parse(priceRaw);
        IElementHandle nameElement = await element.QuerySelectorAsync("a");
        string name = await nameElement.InnerTextAsync();
        IElementHandle urlElement = await element.QuerySelectorAsync("a");
        string url = await urlElement.GetAttributeAsync("href");
        return new Product(name, url, price);
    }
}
