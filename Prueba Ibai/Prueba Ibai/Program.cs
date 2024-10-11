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
        await page.GotoAsync("https://pokebank.es");

        //IElementHandle? acceptButton = await page.QuerySelectorAsync("#CookiesConsent > div > div > form > div > button");
        //if (acceptButton != null) await acceptButton.ClickAsync();

        // Escribimos en la barra de búsqueda lo que queremos buscar
        // Para conseguir el enlace, inspeccionamos en la zona
        IElementHandle buscar = await page.QuerySelectorAsync("#menu-1-c9878bc > li.astm-search-menu.is-menu.is-dropdown.menu-item > a > svg > path");
        await buscar.ClickAsync();
        IElementHandle searchInput = await page.QuerySelectorAsync("#is-search-input-24930");
        await searchInput.FillAsync("sobres de pokemon");

        // Le damos al botón de buscar
        IElementHandle searchButton = await page.QuerySelectorAsync("#menu-1-c9878bc > li.astm-search-menu.is-menu.is-dropdown.menu-item > form > button > span.is-search-icon");
        await searchButton.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // Recorremos la lista de productos y recolectamos los datos
        List<Product> products = new List<Product>();
        IReadOnlyList<IElementHandle> productElements = await page.QuerySelectorAllAsync("#main > div > ul");

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
        IElementHandle priceElement = await element.QuerySelectorAsync("li.member-discount.discount-restricted.ast-grid-common-col.ast-full-width.ast-article-post.desktop-align-left.tablet-align-left.mobile-align-left.product.type-product.post-18243.status-publish.instock.product_cat-destacados.product_cat-ingles.product_cat-no-limite-pro.product_cat-sobre.has-post-thumbnail.shipping-taxable.purchasable.product-type-simple > div.astra-shop-summary-wrap > span.price > span > bdi");
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