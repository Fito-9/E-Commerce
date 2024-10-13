using System.Diagnostics;
using System.Text;
using System.Xml;
using Microsoft.Playwright;
using static System.Net.Mime.MediaTypeNames;
namespace PlayWrigth_Sobres;

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
        await page.GotoAsync("https://www.idealo.es/");

        IElementHandle? acceptButton = await page.QuerySelectorAsync("#accept");
        if (acceptButton != null) await acceptButton.ClickAsync();

        // Escribimos en la barra de búsqueda lo que queremos buscar
        // Para conseguir el enlace, inspeccionamos en la zona
        IElementHandle searchInput = await page.QuerySelectorAsync("#i-search-input");
        await searchInput.FillAsync("Sobres Pokemon");

        // Le damos al botón de buscar
        IElementHandle searchButton = await page.QuerySelectorAsync("#i-header-search > button.i-search-button.i-search-button--submit > span > svg");
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


        Console.WriteLine($"La oferta más barata es: {cheapest}");
        Console.WriteLine($"La oferta más cara es: {expensive}");
        

    }
    private static async Task<Product> GetProductAsync(IElementHandle element)
    {
        IElementHandle priceElement = await element.QuerySelectorAsync(".col-price.pe-sm-2");
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