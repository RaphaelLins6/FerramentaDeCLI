using System.Diagnostics;
using Spectre.Console;
using System.Runtime.InteropServices;

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
            { "Java Runtime Environment", "Oracle.JavaRuntimeEnvironment" },

            // Office
            { "Microsoft Office 365", "Microsoft.Office" }
        };

        private readonly bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        // --- MÉTODOS DE INTERFACE (MENUS) ---

        public void ExibirMenuOtimizacoes()
        {
            AnsiConsole.Clear();
            // Título padronizado com Figlet
            AnsiConsole.Write(new FigletText("Otimizacao").Centered().Color(Color.Cyan1));
            AnsiConsole.WriteLine();

            var escolha = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Selecione a otimização desejada (Exclusivo Windows):[/]")
                    .AddChoices(new[] { 
                        "Limpeza de Disco", 
                        "Reparo (SFC/DISM)", 
                        "Ativar Desempenho Máximo", 
                        "Ativar Massgrave", 
                        "Voltar" 
                    }));

            switch (escolha)
            {
                case "Limpeza de Disco":
                    LimparArquivosTemporarios();
                    break;
                case "Reparo (SFC/DISM)":
                    RepararSistema();
                    break;
                case "Ativar Desempenho Máximo":
                    AtivarDesempenhoMaximo();
                    break;
                case "Ativar Massgrave":
                    AbrirMassgrave();
                    break;
            }

            if (escolha != "Voltar")
            {
                AnsiConsole.MarkupLine("\n[grey]Pressione qualquer tecla para voltar ao menu...[/]");
                Console.ReadKey();
                ExibirMenuOtimizacoes();
            }
        }

        public void InstalarSoftwares()
        {
            if (!_isWindows)
            {
                AnsiConsole.MarkupLine("[red]A instalação via Winget só está disponível no Windows.[/]");
                AnsiConsole.MarkupLine("[yellow]Dica: No Linux, use o gerenciador de pacotes do seu sistema (apt).[/]");
                Console.ReadKey();
                return;
            }

            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("Instalar").Centered().Color(Color.Green));
            AnsiConsole.WriteLine();

            var opcoesMenu = new List<string> 
            { 
                "[red]<- Voltar ao Menu Anterior[/]",
                "[cyan]▲ INSTALAR TUDO DA LISTA[/]"
            };
            opcoesMenu.AddRange(_programasDisponiveis.Keys);

            var selecionados = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("Selecione os programas que deseja [green]instalar[/]")
                    .NotRequired()
                    .PageSize(15)
                    .InstructionsText("[grey]([blue]<espaço>[/] p/ selecionar, [green]<enter>[/] p/ confirmar)[/]")
                    .AddChoices(opcoesMenu));

            if (selecionados.Contains("[red]<- Voltar ao Menu Anterior[/]") || selecionados.Count == 0) return;

            IEnumerable<string> listaParaInstalar = selecionados.Contains("[cyan]▲ INSTALAR TUDO DA LISTA[/]") 
                ? _programasDisponiveis.Keys 
                : selecionados;

            foreach (var nome in listaParaInstalar)
            {
                if (_programasDisponiveis.TryGetValue(nome, out string? id))
                {
                    ExecutarInstalacaoWinget(nome, id);
                }
            }

            AnsiConsole.MarkupLine("\n[green]Processo finalizado![/]");
            Console.ReadKey();
        }

        public void DiagnosticoRede()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("Rede").Centered().Color(Color.Blue));
            AnsiConsole.WriteLine();

            var comando = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Selecione o teste ou ação de rede:[/]")
                    .PageSize(10)
                    .AddChoices(new[] {
                        "Teste de Latência (Ping 8.8.8.8)",
                        "Rastrear Rota (Tracert/Traceroute)",
                        "Teste de Gateway Local (Ping Roteador)",
                        "Verificar DNS (NSLookup)",
                        "Limpar Cache DNS (Flush DNS)",
                        "Informações de Interface (IPConfig/IP A)",
                        "Voltar"
                    }));

            if (comando == "Voltar") return;

            string cmdExecutar = "";
            bool precisaDeAdmin = false;

            switch (comando)
            {
                case "Teste de Latência (Ping 8.8.8.8)":
                    cmdExecutar = _isWindows ? "ping 8.8.8.8 -n 10" : "ping 8.8.8.8 -c 10";
                    break;

                case "Rastrear Rota (Tracert/Traceroute)":
                    cmdExecutar = _isWindows ? "tracert 8.8.8.8" : "traceroute 8.8.8.8";
                    break;

                case "Teste de Gateway Local (Ping Roteador)":
                    // Tenta o IP padrão mais comum, mas no Windows o ideal seria buscar o gateway dinamicamente
                    cmdExecutar = _isWindows ? "ping 192.168.1.1 -n 5" : "ping 192.168.1.1 -c 5";
                    break;

                case "Verificar DNS (NSLookup)":
                    cmdExecutar = "nslookup google.com";
                    break;

                case "Limpar Cache DNS (Flush DNS)":
                    cmdExecutar = _isWindows ? "ipconfig /flushdns" : "sudo resolvectl flush-caches || sudo systemd-resolve --flush-caches";
                    precisaDeAdmin = true;
                    break;

                case "Informações de Interface (IPConfig/IP A)":
                    cmdExecutar = _isWindows ? "ipconfig /all" : "ip a";
                    break;
            }

            if (!string.IsNullOrEmpty(cmdExecutar))
            {
                AnsiConsole.MarkupLine($"\n[yellow]Executando:[/] [white]{cmdExecutar}[/]\n");
                
                // Se for Flush DNS no Windows, podemos rodar silencioso ou interativo
                if (comando == "Limpar Cache DNS (Flush DNS)" && _isWindows)
                {
                    ExecutarComandoSimples(cmdExecutar);
                    AnsiConsole.MarkupLine("[green]Cache DNS limpo com sucesso![/]");
                }
                else
                {
                    ExecutarComandoInterativo(cmdExecutar);
                }
            }

            AnsiConsole.MarkupLine("\n[grey]Pressione qualquer tecla para voltar ao menu de redes...[/]");
            Console.ReadKey();
            DiagnosticoRede(); // Retorna ao menu de redes
        }

        // --- LÓGICA DE EXECUÇÃO ---

        private void ExecutarInstalacaoWinget(string nome, string id)
        {
            AnsiConsole.Status().Start($"Instalando [bold blue]{nome}[/]...", ctx =>
            {
                try
                {
                    ProcessStartInfo psi = new()
                    {
                        FileName = "winget",
                        Arguments = $"install --id {id} --silent --accept-package-agreements --accept-source-agreements",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var processo = Process.Start(psi);
                    processo?.WaitForExit();

                    if (processo?.ExitCode == 0)
                        AnsiConsole.MarkupLine($"[[[green]OK[/]]] {nome} instalado.");
                    else
                        AnsiConsole.MarkupLine($"[[[red]ERRO[/]]] Falha em {nome}. Código: {processo?.ExitCode}");
                }
                catch (Exception ex) { AnsiConsole.MarkupLine($"[[[red]FALHA[/]]] {nome}: {ex.Message}"); }
            });
        }

        public void LimparArquivosTemporarios()
        {
            if (_isWindows)
            {
                var drives = DriveInfo.GetDrives().Where(d => d.IsReady).Select(d => d.Name).ToList();
                drives.Add("Limpar Todos");

                var escolha = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("[yellow]Disco para limpeza:[/]").AddChoices(drives));

                AnsiConsole.Status().Start("Limpando temporários...", ctx =>
                {
                    string[] pastas = { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch"), Path.GetTempPath(), @"C:\Windows\Temp" };
                    foreach (var pasta in pastas)
                    {
                        try {
                            DirectoryInfo di = new(pasta);
                            foreach (FileInfo file in di.GetFiles()) try { file.Delete(); } catch { }
                            foreach (DirectoryInfo dir in di.GetDirectories()) try { dir.Delete(true); } catch { }
                        } catch { }
                    }

                    if (escolha == "Limpar Todos") ExecutarComandoSimples("cleanmgr /autoclean");
                    else ExecutarComandoSimples($"cleanmgr /d {escolha.Substring(0, 2)} /sagerun:1");
                });
            }
            else
            {
                ExecutarComandoSimples("sudo rm -rf /tmp/* && sudo apt-get clean -y");
            }
            AnsiConsole.MarkupLine("[green]Limpeza concluída![/]");
        }

        public void RepararSistema()
        {
            if (!_isWindows) return;
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("Reparo").Centered().Color(Color.Cyan1));
            
            AnsiConsole.Status().Start("Trabalhando na manutenção...", ctx => {
                ctx.Status("SFC Scannow..."); ExecutarComandoSimples("sfc /scannow");
                ctx.Status("DISM RestoreHealth..."); ExecutarComandoSimples("dism /online /cleanup-image /restorehealth");
            });
            AnsiConsole.MarkupLine("[bold green]Reparo finalizado![/]");
        }

        public void AtivarDesempenhoMaximo()
        {
            if (!_isWindows) return;
            string guid = "e9a42b02-d5df-448d-aa00-03f14749eb61";
            ExecutarComandoSimples($"powercfg -duplicatescheme {guid}");
            ExecutarComandoSimples($"powercfg /setactive {guid}");
            AnsiConsole.MarkupLine("[green]Desempenho Máximo Ativado![/]");
        }

        public void AbrirMassgrave()
        {
            if (!_isWindows) return;
            ProcessStartInfo psi = new() {
                FileName = "powershell",
                Arguments = "-Command \"irm https://get.activated.win | iex\"",
                UseShellExecute = true,
                Verb = "runas"
            };
            Process.Start(psi);
        }

        private void ExecutarComandoSimples(string comando)
        {
            string shell = _isWindows ? "cmd.exe" : "/bin/bash";
            string args = _isWindows ? $"/c {comando}" : $"-c \"{comando}\"";
            ProcessStartInfo psi = new() { FileName = shell, Arguments = args, CreateNoWindow = true, UseShellExecute = false };
            try { using var p = Process.Start(psi); p?.WaitForExit(); } catch { }
        }

        private void ExecutarComandoInterativo(string comando)
        {
            string shell = _isWindows ? "cmd.exe" : "/bin/bash";
            string args = _isWindows ? $"/c {comando}" : $"-c \"{comando}\"";
            ProcessStartInfo psi = new() { FileName = shell, Arguments = args, UseShellExecute = false, CreateNoWindow = false };
            Process.Start(psi)?.WaitForExit();
        }
    }
}