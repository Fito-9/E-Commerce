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
        //# df-result-product-47cfa770f13a872404130b54af41a573-options > div.dfd-card-pricing
        await using IBrowser browser = await
        playwright.Chromium.LaunchAsync(options);
        await using IBrowserContext context = await browser.NewContextAsync();
        IPage page = await context.NewPageAsync();

        // Ir a la página de CardMarket
        await page.GotoAsync("https://xtremmedia.com");

        //IElementHandle? acceptButton = await page.QuerySelectorAsync("#ecwid-products > div > div > div > div > div > div:nth-child(2) > div > div.ec-store.ec-store--no-transition > div > div > div > div > div.ec-notice__message > div.ec-notice__buttons.ec-notice__buttons--fullwidth > div.ec-notice__button.ec-notice__button--ok > div > button");
        //if (acceptButton != null) await acceptButton.ClickAsync();
        //IElementHandle? notificaciones = await page.QuerySelectorAsync("#alert_box_btn_close");
        //if(notificaciones != null) await notificaciones.ClickAsync();
        // Escribimos en la barra de búsqueda lo que queremos buscar
        // Para conseguir el enlace, inspeccionamos en la zona
        //IElementHandle buscar = await page.QuerySelectorAsync("#tvcms-mobile-view-header > div.tvcmsheader-sticky > div.tvcmsmobile-header-search-logo-wrapper > div.tvcmsmobile-header-search.col-md-4.col-sm-4 > div.tvmobile-search-icon");
        //await buscar.ClickAsync();
        IElementHandle searchInput = await page.QuerySelectorAsync("#_desktop_search > div > div > div.tvsearch-header-display-wrappper.tvsearch-header-display-full > form > div.tvheader-top-search > div > input");
        await searchInput.FillAsync("sobre pokemon");

        // Le damos al botón de buscar
        IElementHandle searchButton = await page.QuerySelectorAsync("#_desktop_search > div > div > div.tvsearch-header-display-wrappper.tvsearch-header-display-full > form > div.tvheader-top-search-wrapper");
        await searchButton.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // Recorremos la lista de productos y recolectamos los datos
        List<Product> products = new List<Product>();
        IReadOnlyList<IElementHandle> productElements = await page.QuerySelectorAllAsync("#dfd-results-ymgrZ");

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
        IElementHandle priceElement = await element.QuerySelectorAsync(".dfd-card-price");
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
