using Spectre.Console;
using ToolManutencao.Services;

var hardwareService = new HardwareService();
var automationService = new AutomationService();
bool emExecucao = true;

while (emExecucao)
{
    AnsiConsole.Clear();
    AnsiConsole.Write(new FigletText("Ferramenta de Manutenção").Centered().Color(new Color(255, 135, 0)));

    // Menu de Seleção
    var opcao = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[yellow]O que deseja fazer hoje?[/]")
            .PageSize(10)
            .AddChoices(new[] {
                "Ver Informações do Hardware",
                "Instalar Softwares Básicos",
                "Testes de Hardware",
                "Otimizações de Sistema",
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

        case "Instalar Softwares Básicos":
            automationService.InstalarSoftwares();
            break;
        
        case "Testes de Hardware":
            bool emSubMenu = true;
            while (emSubMenu)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(new FigletText("Testes").Justify(Justify.Left).Color(Color.Orange1));

                var testeOpcao = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Selecione o teste desejado:[/]")
                        .AddChoices(new[] {
                            "Verificar Saúde dos Discos (S.M.A.R.T)",
                            "Teste de Estresse de CPU/RAM (Em breve)",
                            "Voltar ao Menu Principal"
                        }));

                switch (testeOpcao)
                {
                    case "Verificar Saúde dos Discos (S.M.A.R.T)":
                        // Chamando a sua nova função
                        hardwareService.ExibirSaudeDiscos(); 
                        AnsiConsole.MarkupLine("\n[grey]Pressione qualquer tecla para voltar...[/]");
                        Console.ReadKey();
                        break;

                    case "Teste de Estresse de CPU/RAM (Em breve)":
                        AnsiConsole.MarkupLine("[blue]Esta função será implementada no próximo módulo![/]");
                        Console.ReadKey();
                        break;

                    case "Voltar ao Menu Principal":
                        emSubMenu = false;
                        break;
                }
            }
            break;

        case "Otimizações de Sistema":
            var opt = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Selecione a otimização:[/]")
                    .AddChoices(new[] { 
                        "Ativar Desempenho Máximo", 
                        "Ativar Windows/Office (Massgrave)", 
                        "Voltar" 
                    }));

            if (opt == "Ativar Desempenho Máximo") automationService.AtivarDesempenhoMaximo();
            if (opt == "Ativar Windows/Office (Massgrave)") automationService.AbrirMassgrave();
            break;

        case "Sair":
            emExecucao = false;
            break;
    }
}