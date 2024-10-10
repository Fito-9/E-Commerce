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
        // Ir a la página de fnac
        await page.GotoAsync("https://www.pokemillon.com");

       
        IElementHandle buscar = await page.QuerySelectorAsync("#shopify-section-header > header > nav > div.d-none.d-lg-flex.mt-0.col-1.flex-wrap.justify-content-end.align-items-center.et-header__icons.et-header_icons-account-enable > div");
        await buscar.ClickAsync();
        // Escribimos en la barra de búsqueda lo que queremos buscar
        IElementHandle searchInput = await page.QuerySelectorAsync("#et-search_popup > div > div > div > div.et-search__popup.d-flex.align-items-center > form > input");
        await searchInput.FillAsync("sobre");
        // Le damos al botón de buscar
        IElementHandle searchButton = await page.QuerySelectorAsync("#et-search_popup > div > div > div > div.et-search__popup.d-flex.align-items-center > form > button");
        await searchButton.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        // Recorremos la lista de productos y recolectamos los datos
        List<Product> products = new List<Product>();
        IReadOnlyList<IElementHandle> productElements = await
        page.QuerySelectorAllAsync("#PageContainer > main > div.container.et-list-view-items");
        /*
        int contador = 0;
        foreach (IElementHandle productElement in productElements)
        {
            if (contador >= 10)
            {
                break;
            }
            try
            {
                Product product = await GetProductAsync(productElement);
                products.Add(product);
                Console.WriteLine(product);
            }
            catch { }
            contador++;
        }
        */
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
        Console.WriteLine("Los 10 primeros productos encontrados:");
        foreach (var product in products)
        {
            Console.WriteLine(product);
        }
        //ProcessStartInfo processInfo = new ProcessStartInfo()
     
    }
    private static async Task<Product> GetProductAsync(IElementHandle element)
    {
        IElementHandle priceElement= await element.QuerySelectorAsync("a:nth-child(2) > div.et-list__price > dl > div.price__regular > dd > span"); ;
        IElementHandle preciorebaja=await element.QuerySelectorAsync("a:nth-child(3) > div.et-list__price > dl > div.price__sale > dd > span");
        
        string priceRaw;
        decimal price;
        if (priceElement != null)
        {
             
        
            priceRaw = await priceElement.InnerTextAsync();
        }
        else
        {
            
            
            priceRaw = await preciorebaja.InnerTextAsync();
           
        }
        priceRaw = priceRaw.Replace("EUR", "",StringComparison.OrdinalIgnoreCase);
        priceRaw = priceRaw.Trim();
        price = decimal.Parse(priceRaw);
        IElementHandle nameElement = await
        element.QuerySelectorAsync(".s-item__title");
        string name = await nameElement.InnerTextAsync();
        IElementHandle urlElement = await element.QuerySelectorAsync("a");
        string url = await urlElement.GetAttributeAsync("href");
        return new Product(name, url, price);
    }

}
public class Product
{
    public string Name { get; init; }
    public string Url { get; init; }
    public decimal Price { get; init; }
    public Product(string name, string url, decimal price)
    {
        Name = name;
        Url = url;
        Price = price;
    }
}