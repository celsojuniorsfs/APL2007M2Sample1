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
    // Caixa de texto para exibir os resultados dos downloads
    private TextBox _resultsTextBox;
    
    // Botão para iniciar o processo de download
    private Button _startButton;
    
    // HttpClient configurado para limitar o tamanho máximo do buffer de resposta
    private readonly HttpClient _client = new HttpClient { MaxResponseContentBufferSize = 1_000_000 };

    // Lista de URLs a serem processadas
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

    // Evento de clique do botão de início
    private void OnStartButtonClick(object sender, RoutedEventArgs e)
    {
        // Desabilita o botão de início e limpa a caixa de texto de resultados
        _startButton.IsEnabled = false;
        _resultsTextBox.Clear();

        // Inicia a tarefa assíncrona para somar os tamanhos das páginas
        Task.Run(() => StartSumPageSizesAsync());
    }

    // Inicia a soma dos tamanhos das páginas de forma assíncrona
    private async Task StartSumPageSizesAsync()
    {
        // Chama o método para somar os tamanhos das páginas
        await SumPageSizesAsync();
        
        // Atualiza a interface do usuário para indicar que o controle retornou e reabilita o botão de início
        await Dispatcher.BeginInvoke(() =>
        {
            _resultsTextBox.Text += $"\nControl returned to {nameof(OnStartButtonClick)}.";
            _startButton.IsEnabled = true;
        });
    }

    // Soma os tamanhos das páginas baixadas de forma assíncrona
    private async Task SumPageSizesAsync()
    {
        // Inicia um cronômetro para medir o tempo de execução
        var stopwatch = Stopwatch.StartNew();

        // Cria uma coleção de tarefas para processar cada URL
        IEnumerable<Task<int>> downloadTasksQuery =
            from url in _urlList
            select ProcessUrlAsync(url, _client);

        // Converte a consulta em um array de tarefas
        Task<int>[] downloadTasks = downloadTasksQuery.ToArray();

        // Aguarda todas as tarefas serem concluídas e soma os tamanhos dos conteúdos baixados
        int[] lengths = await Task.WhenAll(downloadTasks);
        int total = lengths.Sum();

        // Atualiza a interface do usuário com o total de bytes retornados e o tempo decorrido
        await Dispatcher.BeginInvoke(() =>
        {
            stopwatch.Stop();

            _resultsTextBox.Text += $"\nTotal bytes returned:  {total:#,#}";
            _resultsTextBox.Text += $"\nElapsed time:          {stopwatch.Elapsed}\n";
        });
    }

    // Processa uma URL de forma assíncrona, baixando seu conteúdo e retornando seu tamanho
    private async Task<int> ProcessUrlAsync(string url, HttpClient client)
    {
        try
        {
            // Baixa o conteúdo da URL como um array de bytes
            byte[] byteArray = await client.GetByteArrayAsync(url);
            
            // Exibe os resultados na interface do usuário
            await DisplayResultsAsync(url, byteArray);
            
            // Retorna o tamanho do conteúdo baixado
            return byteArray.Length;
        }
        catch (HttpRequestException e)
        {
            // Captura exceções específicas de requisições HTTP e exibe uma mensagem de erro
            await Dispatcher.BeginInvoke(() =>
            {
                _resultsTextBox.Text += $"\nError downloading {url}: {e.Message}\n";
            });
            return 0; // Retorna 0 para indicar que o download falhou
        }
        catch (Exception e)
        {
            // Captura quaisquer outras exceções inesperadas e exibe uma mensagem de erro
            await Dispatcher.BeginInvoke(() =>
            {
                _resultsTextBox.Text += $"\nUnexpected error downloading {url}: {e.Message}\n";
            });
            return 0; // Retorna 0 para indicar que o download falhou
        }
    }

    // Exibe os resultados do download na interface do usuário
    private Task DisplayResultsAsync(string url, byte[] content) =>
        Dispatcher.BeginInvoke(() =>
            _resultsTextBox.Text += $"{url,-60} {content.Length,10:#,#}\n")
                  .Task;

    // Garante que o HttpClient seja descartado quando a janela for fechada
    protected override void OnClosed(EventArgs e) => _client.Dispose();
}