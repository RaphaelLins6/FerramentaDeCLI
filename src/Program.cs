using Spectre.Console;
using ToolManutencao.Services;

var hardwareService = new HardwareService();
var automationService = new AutomationService();
bool emExecucao = true;

while (emExecucao)
{
    AnsiConsole.Clear();
    AnsiConsole.Write(new FigletText("Ferramenta de Manutenção de TI").Centered().Color(new Color(255, 135, 0)));
    AnsiConsole.MarkupLine("[grey]Desenvolvido por [white]Raphael Lins \u00ae[/] - 2026[/]");    
    AnsiConsole.WriteLine();

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

    AnsiConsole.MarkupLine("\n[grey]Copyright \u00ae Raphael Lins. Todos os direitos reservados.[/]");    
    
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
                    .Title("[yellow]Selecione a ferramenta de otimização:[/]")
                    .AddChoices(new[] { 
                        "Limpeza de Disco e Arquivos Temporários",
                        "Gerenciamento e Reparo do Sistema (SFC/DISM)",
                        "Ativar Desempenho Máximo (Plano de Energia)", 
                        "Ativar Windows/Office (Massgrave)", 
                        "Voltar" 
                    }));

            switch(opt)
            {
                case "Limpeza de Disco e Arquivos Temporários":
                    automationService.LimparArquivosTemporarios();
                    break;
                case "Gerenciamento e Reparo do Sistema (SFC/DISM)":
                    automationService.RepararSistema();
                    break;
                case "Ativar Desempenho Máximo (Plano de Energia)":
                    automationService.AtivarDesempenhoMaximo();
                    break;
                case "Ativar Windows/Office (Massgrave)":
                    automationService.AbrirMassgrave();
                    break;
            }
            
            if (opt != "Voltar")
            {
                AnsiConsole.MarkupLine("\n[grey]Operação concluída. Pressione qualquer tecla...[/]");
                Console.ReadKey();
            }
            break;

        case "Sair":
            emExecucao = false;
            break;
    }
}