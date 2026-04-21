using Spectre.Console;
using ToolManutencao.Services;
using System.Runtime.InteropServices; // Necessário para detectar o SO

var hardwareService = new HardwareService();
var automationService = new AutomationService();
bool emExecucao = true;

// Detecta o sistema uma vez para usar no menu
bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

while (emExecucao)
{
    AnsiConsole.Clear();
    AnsiConsole.Write(new FigletText("Ferramenta de Manutenção").Centered().Color(Color.Orange1));
    
    // Mostra o SO atual no cabeçalho
    string soNome = isWindows ? "Windows" : "Linux";
    AnsiConsole.MarkupLine($"[grey]Sistema Detectado:[/] [blue]{soNome}[/] | [grey]Desenvolvido por Raphael Lins[/]");
    AnsiConsole.WriteLine();

    var menuPrincipal = new SelectionPrompt<string>()
            .Title("[yellow]O que deseja fazer hoje?[/]")
            .PageSize(10);

    // Adiciona opções básicas
    menuPrincipal.AddChoices("Ver Informações do Hardware", "Ferramentas de Rede");

    // Adiciona opções específicas de Windows
    if (isWindows) {
        menuPrincipal.AddChoices("Instalar Softwares Básicos", "Testes de Hardware", "Otimizações de Sistema");
    } else {
        // No Linux, você pode adicionar opções futuras específicas como "Limpeza via APT"
        menuPrincipal.AddChoices("Limpeza de Sistema (Linux)");
    }
    
    menuPrincipal.AddChoices("Sair");

    var opcao = AnsiConsole.Prompt(menuPrincipal);

    switch (opcao)
    {
        case "Ver Informações do Hardware":
            AnsiConsole.Status().Start("Lendo hardware...", ctx => {
                var table = hardwareService.GetHardwareTable();
                AnsiConsole.Write(table);
            });
            AnsiConsole.MarkupLine("\n[grey]Pressione qualquer tecla para voltar...[/]");
            Console.ReadKey();
            break;

        case "Otimizações de Sistema":
            if (isWindows) 
            {
                // Chame o método do Service, ele já limpa a tela e coloca o título "Otimizacao"
                automationService.ExibirMenuOtimizacoes(); 
            }
            break;

        case "Ferramentas de Rede":
            automationService.DiagnosticoRede(); // Este método precisa ser ajustado no Service
            break;

        case "Instalar Softwares Básicos":
            automationService.InstalarSoftwares(); // Este método deve ter o Figlet
            break;

        case "Testes de Hardware":
            hardwareService.ExibirMenuTestes(); // Este método deve ter o Figlet
            break;

        case "Limpeza de Sistema (Linux)":
            automationService.LimparArquivosTemporarios();
            break;

        case "Sair":
            emExecucao = false;
            break;
    }
}