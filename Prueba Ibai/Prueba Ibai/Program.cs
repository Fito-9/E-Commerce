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

        // Ir a la página de CardMarket
        await page.GotoAsync("https://www.google.com/url?sa=t&rct=j&q=&esrc=s&source=web&cd=&ved=2ahUKEwiy9YzWw4iJAxVU1QIHHe4MFmAQFnoECCwQAQ&url=https%3A%2F%2Fwww.mtgmetropolis.com%2Fmetropolistienda%2Fshow_category.asp%3Fcategory%3D44%26Ampliacion%3DCajas%2520y%2520Sobres&usg=AOvVaw0fntyplDObbDUUSqXM3X7q&opi=89978449");

        //IElementHandle? acceptButton = await page.QuerySelectorAsync("#CookiesConsent > div > div > form > div > button");
        //if (acceptButton != null) await acceptButton.ClickAsync();

        // Escribimos en la barra de búsqueda lo que queremos buscar
        // Para conseguir el enlace, inspeccionamos en la zona
        //IElementHandle buscar = await page.QuerySelectorAsync("#menu-1-c9878bc > li.astm-search-menu.is-menu.is-dropdown.menu-item > a > svg > path");
        //await buscar.ClickAsync();
        IElementHandle searchInput = await page.QuerySelectorAsync("body > div > table:nth-child(1) > tbody > tr:nth-child(2) > td:nth-child(2) > table > tbody > tr > td:nth-child(4) > table > tbody > tr:nth-child(2) > td > table > tbody > tr:nth-child(2) > td:nth-child(4) > input.textfield_buscador");
        await searchInput.FillAsync("pokemon");

        // Le damos al botón de buscar
        IElementHandle searchButton = await page.QuerySelectorAsync("body > div > table:nth-child(1) > tbody > tr:nth-child(2) > td:nth-child(2) > table > tbody > tr > td:nth-child(4) > table > tbody > tr:nth-child(2) > td > table > tbody > tr:nth-child(2) > td:nth-child(5) > input[type=image]");
        await searchButton.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // Recorremos la lista de productos y recolectamos los datos
        List<Product> products = new List<Product>();
        IReadOnlyList<IElementHandle> productElements = await page.QuerySelectorAllAsync("body > div > table:nth-child(1) > tbody > tr:nth-child(2) > td:nth-child(2) > table > tbody > tr > td:nth-child(2) > table > tbody > tr > td > table:nth-child(3)");

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

    }
    private static async Task<Product> GetProductAsync(IElementHandle element)
    {
        IElementHandle priceElement = await element.QuerySelectorAsync("table > tbody > tr > td:nth-child(2) > table > tbody > tr > td > table:nth-child(3) > tbody > tr > td > table > tbody > tr:nth-child(2) > td > table > tbody > tr:nth-child(1) > td > table > tbody > tr > td.blanco > a.precio_listado");
        string priceRaw = await priceElement.InnerTextAsync();
        priceRaw = priceRaw.Replace(" €", "", StringComparison.OrdinalIgnoreCase);
        priceRaw = priceRaw.Trim();
        decimal price = decimal.Parse(priceRaw);

        //En la entrega dice que no son necesarios ni el nombre ni la URL del producto, así que ni los llamamos.
        string name = "";
        string url = "";
        return new Product(name, url, price);
    }
}
