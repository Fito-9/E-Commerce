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
        await page.GotoAsync("https://www.cardmarket.com/es/Pokemon");

        IElementHandle? acceptButton = await page.QuerySelectorAsync("#CookiesConsent > div > div > form > div > button");
        if (acceptButton != null) await acceptButton.ClickAsync();

        // Escribimos en la barra de búsqueda lo que queremos buscar
        // Para conseguir el enlace, inspeccionamos en la zona
        IElementHandle searchInput = await page.QuerySelectorAsync("#ProductSearchInput");
        await searchInput.FillAsync("sobres");

        // Le damos al botón de buscar
        IElementHandle searchButton = await page.QuerySelectorAsync("#search-btn");
        await searchButton.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // Recorremos la lista de productos y recolectamos los datos
        List<Product> products = new List<Product>();
        IReadOnlyList<IElementHandle> productElements = await page.QuerySelectorAllAsync("body > main > section > div.table.table-striped > div.table-body > div ");

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
        // Con los datos recolectados, buscamos el producto más barato
        Product cheapest = products.MinBy(p => p.Price);
        Product expensive = products.MaxBy(p => p.Price);

        decimal average = products.Average(p => p.Price);   

        Console.WriteLine($"La oferta más barata es: {cheapest}");
        Console.WriteLine($"La oferta más cara es: {expensive}");
        Console.WriteLine($"La media de los precios de los productos es: {average}");
        // Abrimos el navegador con la oferta más barata

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
        IElementHandle priceElement = await element.QuerySelectorAsync(".col-price.pe-sm-2");
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
