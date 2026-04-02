using Spectre.Console;
using ToolManutencao.Services;

var hardwareService = new HardwareService();
bool emExecucao = true;

while (emExecucao)
{
    AnsiConsole.Clear();
    AnsiConsole.Write(new FigletText("Service Tool").Centered().Color(new Color(255, 135, 0)));

    // Menu de Seleção
    var opcao = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[yellow]O que deseja fazer hoje?[/]")
            .PageSize(10)
            .AddChoices(new[] {
                "Ver Informações do Hardware",
                "Instalar Softwares Básicos (Winget)",
                "Sair"
            }));

    switch (opcao)
    {
        case "Ver Informações do Hardware":
            AnsiConsole.Status().Start("Lendo hardware...", ctx => {
                var table = hardwareService.GetHardwareTable();
                AnsiConsole.Write(table);
            });
            AnsiConsole.MarkupLine("\n[grey]Pressione qualquer tecla para voltar ao menu...[/]");
            Console.ReadKey();
            break;

        case "Instalar Softwares Básicos (Winget)":
            AnsiConsole.MarkupLine("[red]Em breve: Integração com Winget![/]");
            Console.ReadKey();
            break;

        case "Sair":
            emExecucao = false;
            break;
    }
}