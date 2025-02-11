# APL2007M2Sample1

Este projeto é um exemplo de aplicativo WPF (Windows Presentation Foundation) que demonstra como realizar operações assíncronas em paralelo para baixar o conteúdo de várias URLs e exibir os resultados em uma interface gráfica.

## Estrutura do Projeto

- **APL2007M2Sample1.csproj**: Arquivo de projeto que define o aplicativo como um WinExe direcionado ao .NET 6.0 com suporte a WPF.
- **APL2007M2Sample1.sln**: Arquivo de solução do Visual Studio.
- **App.xaml**: Define a aplicação WPF e especifica que a janela principal é `MainWindow.xaml`.
- **App.xaml.cs**: Código-behind do `App.xaml`, define a classe `App` que herda de `Application`.
- **MainWindow.xaml**: Define a interface gráfica da janela principal com um botão e uma caixa de texto.
- **MainWindow.xaml.cs**: Código-behind do `MainWindow.xaml`, define a lógica do aplicativo.
- **bin/** e **obj/**: Diretórios gerados pelo build contendo arquivos binários e intermediários.

## Funcionalidades

- **Baixar Conteúdo de URLs**: O aplicativo baixa o conteúdo de várias URLs de forma assíncrona e paralela.
- **Exibir Resultados**: Os resultados dos downloads são exibidos em uma caixa de texto na interface do usuário.
- **Tratamento de Exceções**: O aplicativo lida com exceções durante o processo de download e exibe mensagens de erro apropriadas.

## Como Executar

1. **Pré-requisitos**:
    - .NET 6.0 SDK
    - Visual Studio 2022 ou superior

2. **Compilar e Executar**:
    - Abra a solução `APL2007M2Sample1.sln` no Visual Studio.
    - Compile o projeto.
    - Execute o aplicativo.

## Código Principal

### MainWindow.xaml.cs

```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;

public partial class MainWindow : Window
{
    private TextBox _resultsTextBox;
    private Button _startButton;
    private readonly HttpClient _client = new HttpClient { MaxResponseContentBufferSize = 1_000_000 };

    private readonly IEnumerable<string> _urlList = new string[]
    {
        "https://docs.microsoft.com",
        "https://docs.microsoft.com/azure",
        "https://docs.microsoft.com/powershell",
        "https://docs.microsoft.com/dotnet",
        "https://docs.microsoft.com/aspnet/core",
        "https://docs.microsoft.com/windows",
        "https://docs.microsoft.com/office",
        "https://docs.microsoft.com/enterprise-mobility-security",
        "https://docs.microsoft.com/visualstudio",
        "https://docs.microsoft.com/microsoft-365",
        "https://docs.microsoft.com/sql",
        "https://docs.microsoft.com/dynamics365",
        "https://docs.microsoft.com/surface",
        "https://docs.microsoft.com/xamarin",
        "https://docs.microsoft.com/azure/devops",
        "https://docs.microsoft.com/system-center",
        "https://docs.microsoft.com/graph",
        "https://docs.microsoft.com/education",
        "https://docs.microsoft.com/gaming"
    };

    private void OnStartButtonClick(object sender, RoutedEventArgs e)
    {
        _startButton.IsEnabled = false;
        _resultsTextBox.Clear();

        Task.Run(() => StartSumPageSizesAsync());
    }

    private async Task StartSumPageSizesAsync()
    {
        await SumPageSizesAsync();
        await Dispatcher.BeginInvoke(() =>
        {
            _resultsTextBox.Text += $"\nControl returned to {nameof(OnStartButtonClick)}.";
            _startButton.IsEnabled = true;
        });
    }

    private async Task SumPageSizesAsync()
    {
        var stopwatch = Stopwatch.StartNew();

        IEnumerable<Task<int>> downloadTasksQuery =
            from url in _urlList
            select ProcessUrlAsync(url, _client);

        Task<int>[] downloadTasks = downloadTasksQuery.ToArray();

        int[] lengths = await Task.WhenAll(downloadTasks);
        int total = lengths.Sum();

        await Dispatcher.BeginInvoke(() =>
        {
            stopwatch.Stop();

            _resultsTextBox.Text += $"\nTotal bytes returned:  {total:#,#}";
            _resultsTextBox.Text += $"\nElapsed time:          {stopwatch.Elapsed}\n";
        });
    }

    private async Task<int> ProcessUrlAsync(string url, HttpClient client)
    {
        try
        {
            byte[] byteArray = await client.GetByteArrayAsync(url);
            await DisplayResultsAsync(url, byteArray);
            return byteArray.Length;
        }
        catch (HttpRequestException e)
        {
            await Dispatcher.BeginInvoke(() =>
            {
                _resultsTextBox.Text += $"\nError downloading {url}: {e.Message}\n";
            });
            return 0; // Retorna 0 para indicar que o download falhou
        }
        catch (Exception e)
        {
            await Dispatcher.BeginInvoke(() =>
            {
                _resultsTextBox.Text += $"\nUnexpected error downloading {url}: {e.Message}\n";
            });
            return 0; // Retorna 0 para indicar que o download falhou
        }
    }

    private Task DisplayResultsAsync(string url, byte[] content) =>
        Dispatcher.BeginInvoke(() =>
            _resultsTextBox.Text += $"{url,-60} {content.Length,10:#,#}\n")
                  .Task;

    protected override void OnClosed(EventArgs e) => _client.Dispose();
}