using System.Diagnostics;
using Spectre.Console;

namespace ToolManutencao.Services
{
    public class AutomationService
    {
        // Dicionário com Nome Amigável e ID real do Winget
        private readonly Dictionary<string, string> _programasDisponiveis = new()
        {
            // Navegadores
            { "Google Chrome", "Google.Chrome" },
            { "Mozilla Firefox", "Mozilla.Firefox" },

            // Vídeo Chamada
            { "Zoom", "Zoom.Zoom" },
            { "Microsoft Teams", "Microsoft.Teams" },

            // Vídeos
            { "VLC Media Player", "VideoLAN.VLC" },

            // PDF
            { "Adobe Acrobat Reader DC", "Adobe.Acrobat.Reader.64-bit" },

            // Segurança
            { "Panda Antivirus", "PandaSecurity.PandaFreeAntivirus" },

            // Acesso Remoto
            { "AnyDesk", "AnyDeskSoftware.AnyDesk" },
            { "TeamViewer", "TeamViewer.TeamViewer" },

            // Compressão
            { "WinRAR", "RARLab.WinRAR" },

            // Runtime
            { "Java Runtime Environment", "Oracle.JavaRuntimeEnvironment" }
        };

        public void InstalarSoftwares()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[yellow]Instalação Automatizada[/]").RuleStyle("grey").Justify(Justify.Left));

            var opcoesMenu = new List<string> { "[red]<- Voltar ao Menu Anterior[/]" };
            opcoesMenu.AddRange(_programasDisponiveis.Keys);

            var selecionados = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("Selecione os programas que deseja [green]instalar[/]")
                    .NotRequired()
                    .PageSize(12)
                    .InstructionsText("[grey]([blue]<espaço>[/] p/ selecionar, [green]<enter>[/] p/ confirmar)[/]")
                    .AddChoices(opcoesMenu)
                    );

            // SELECIONOU "VOLTAR" OU NADA? SAI DIRETO SEM MENSAGEM.
            if (selecionados.Contains("[red]<- Voltar ao Menu Anterior[/]") || selecionados.Count == 0)
            {
                return; 
            }

            // Só chega aqui se realmente for instalar algo
            foreach (var nome in selecionados)
            {
                string id = _programasDisponiveis[nome];
                ExecutarInstalacaoWinget(nome, id);
            }

            // Apenas após a instalação mostramos o aviso para o técnico ver o que aconteceu
            AnsiConsole.MarkupLine("\n[green]Processo de instalação finalizado![/]");
            AnsiConsole.MarkupLine("[grey]Pressione qualquer tecla para retornar ao menu principal...[/]");
            Console.ReadKey();
        }

        private void ExecutarInstalacaoWinget(string nome, string id)
        {
            AnsiConsole.Status()
                .Start($"Instalando [bold blue]{nome}[/]...", ctx =>
                {
                    try
                    {
                        ProcessStartInfo psi = new()
                        {
                            FileName = "winget",
                            // --silent: sem janelas de instalador
                            // --accept-package-agreements: aceita os termos
                            // --accept-source-agreements: aceita as fontes do winget
                            Arguments = $"install --id {id} --silent --accept-package-agreements --accept-source-agreements",
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        using var processo = Process.Start(psi);
                        processo?.WaitForExit();

                        if (processo?.ExitCode == 0)
                            AnsiConsole.MarkupLine($"[[[green]OK[/]]] {nome} instalado com sucesso.");
                        else
                            AnsiConsole.MarkupLine($"[[[red]ERRO[/]]] Falha ao instalar {nome}. Código: {processo?.ExitCode}");
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[[[red]FALHA[/]]] Erro ao chamar Winget para {nome}: {ex.Message}");
                    }
                });
        }

        public void AtivarDesempenhoMaximo()
        {
            AnsiConsole.Status().Start("Ativando Plano de Desempenho Máximo...", ctx =>
            {
                // Comando oficial do Windows para liberar o plano oculto
                string cmd = "powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61";
                ExecutarComandoSimples(cmd);
                
                // Comando para setar ele como ativo na hora
                ExecutarComandoSimples("powercfg /setactive e9a42b02-d5df-448d-aa00-03f14749eb61");
                
                AnsiConsole.MarkupLine("[[[green]OK[/]]] Plano de [bold]Desempenho Máximo[/] ativado!");
            });
        }

        public void AbrirMassgrave()
        {
            AnsiConsole.MarkupLine("[yellow]Abrindo Massgrave (MAS)...[/]");
            // O MAS roda via um comando PowerShell direto da web
            string comandoPs = "irm https://get.activated.win | iex";
            
            ProcessStartInfo psi = new()
            {
                FileName = "powershell",
                Arguments = $"-Command \"{comandoPs}\"",
                UseShellExecute = true, // MAS precisa de interação, então abrimos a janela
                Verb = "runas" // Garante que abra como admin
            };

            Process.Start(psi);
        }

        // Método auxiliar para comandos que não precisam de janela
        private void ExecutarComandoSimples(string comando)
        {
            ProcessStartInfo psi = new()
            {
                FileName = "cmd.exe",
                Arguments = $"/c {comando}",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process.Start(psi)?.WaitForExit();
        }

    }
}