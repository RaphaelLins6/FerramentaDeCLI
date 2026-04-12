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

        public void InstalarSoftwares()
        {
            if (!_isWindows)
            {
                AnsiConsole.MarkupLine("[red]A instalação via Winget só está disponível no Windows.[/]");
                AnsiConsole.MarkupLine("[yellow]Dica: No Linux, use o gerenciador de pacotes do seu sistema (apt, dnf, pacman).[/]");
                return;
            }
            
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[yellow]Instalação Automatizada[/]").RuleStyle("grey").Justify(Justify.Left));

            // 1. Criamos a lista de opções com "Voltar" e agora "Instalar Todos"
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
                    .AddChoices(opcoesMenu)
                    );

            // 2. Verificação de Saída
            if (selecionados.Contains("[red]<- Voltar ao Menu Anterior[/]") || selecionados.Count == 0)
            {
                return; 
            }

            // 3. Lógica para "Instalar Tudo"
            IEnumerable<string> listaParaInstalar;

            if (selecionados.Contains("[cyan]▲ INSTALAR TUDO DA LISTA[/]"))
            {
                // Se escolheu "Tudo", pegamos todas as chaves do dicionário
                listaParaInstalar = _programasDisponiveis.Keys;
                AnsiConsole.MarkupLine("[yellow]Modo 'Instalar Tudo' ativado![/]\n");
            }
            else
            {
                // Caso contrário, usa apenas o que foi marcado (removendo o separador se ele foi "marcado")
                listaParaInstalar = selecionados.Where(s => s != "--------------------------");
            }

            // 4. Loop de Instalação
            foreach (var nome in listaParaInstalar)
            {
                if (_programasDisponiveis.TryGetValue(nome, out string id))
                {
                    ExecutarInstalacaoWinget(nome, id);
                }
            }

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
            if (_isWindows) {
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
        }

        public void AbrirMassgrave()
        {
            if (_isWindows) {
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
        }

        // Método auxiliar para comandos que não precisam de janela
        private void ExecutarComandoSimples(string comando)
        {
            string shell = _isWindows ? "cmd.exe" : "/bin/bash";
            string args = _isWindows ? $"/c {comando}" : $"-c \"{comando}\"";

            ProcessStartInfo psi = new()
            {
                FileName = shell,
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            try 
            {
                using var processo = Process.Start(psi);
                processo?.WaitForExit();
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Erro ao executar comando:[/] {ex.Message}");
            }
        }

        public void LimparArquivosTemporarios()
        {
            if (_isWindows)
            {
                // 1. Obter discos prontos e adicionar opção global
                var drives = DriveInfo.GetDrives()
                    .Where(d => d.IsReady)
                    .Select(d => d.Name)
                    .ToList();
                
                drives.Add("Limpar Todos");

                var escolha = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Qual disco deseja limpar com o Cleanmgr?[/]")
                        .PageSize(10)
                        .AddChoices(drives));

                AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .Start("Limpando arquivos temporários...", ctx =>
                    {
                        // --- PARTE 1: Limpeza Manual ---
                        string[] pastas = {
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch"),
                            Path.GetTempPath(),
                            @"C:\Windows\Temp"
                        };

                        foreach (var pasta in pastas)
                        {
                            try
                            {
                                ctx.Status($"Acessando: {pasta}");
                                DirectoryInfo di = new DirectoryInfo(pasta);
                                foreach (FileInfo file in di.GetFiles()) { try { file.Delete(); } catch { } }
                                foreach (DirectoryInfo dir in di.GetDirectories()) { try { dir.Delete(true); } catch { } }
                            }
                            catch { /* Silencioso se não houver permissão */ }
                        }

                        // --- PARTE 2: Cleanmgr ---
                        ctx.Status("[bold blue]Iniciando Cleanmgr...[/]");
                        if (escolha == "Limpar Todos")
                        {
                            ExecutarComandoSimples("cleanmgr /autoclean");
                        }
                        else
                        {
                            string driveLetra = escolha.Substring(0, 2);
                            ExecutarComandoSimples($"cleanmgr /d {driveLetra} /sagerun:1");
                        }

                        AnsiConsole.MarkupLine("[[[green]OK[/]]] Limpeza do Windows concluída!");
                    });
            }
            else
            {
                // Lógica para Linux
                AnsiConsole.Status().Start("Limpando temporários do Linux...", ctx => {
                    ctx.Status("Removendo arquivos de /tmp...");
                    ExecutarComandoSimples("sudo rm -rf /tmp/*");
                    
                    ctx.Status("Limpando cache do gerenciador de pacotes...");
                    ExecutarComandoSimples("sudo apt-get clean -y"); 
                    
                    AnsiConsole.MarkupLine("[[[green]OK[/]]] Cache e /tmp limpos no Linux.");
                });
            }
        }

        public void RepararSistema()
        {
            if (_isWindows) {
            AnsiConsole.MarkupLine("[bold yellow]Iniciando Reparo do Sistema e Limpeza de Componentes...[/]");
            AnsiConsole.MarkupLine("[grey]Este processo é profundo e pode demorar alguns minutos. Não feche o terminal.[/]");
            AnsiConsole.WriteLine();

            // 1. SFC Scannow
            AnsiConsole.Status().Start("Executando [cyan]SFC /Scannow[/] (Verificando integridade)...", ctx => 
            {
                ExecutarComandoSimples("sfc /scannow");
                AnsiConsole.MarkupLine("[[[green]OK[/]]] Verificação de arquivos do sistema (SFC) finalizada.");
            });

            // 2. DISM ScanHealth
            AnsiConsole.Status().Start("Executando [cyan]DISM ScanHealth[/] (Procurando corrupção)...", ctx => 
            {
                ExecutarComandoSimples("dism /online /cleanup-image /scanhealth");
                AnsiConsole.MarkupLine("[[[green]OK[/]]] Verificação de imagem (ScanHealth) finalizada.");
            });

            // 3. DISM RestoreHealth
            AnsiConsole.Status().Start("Executando [cyan]DISM RestoreHealth[/] (Reparando imagem)...", ctx => 
            {
                ExecutarComandoSimples("dism /online /cleanup-image /restorehealth");
                AnsiConsole.MarkupLine("[[[green]OK[/]]] Reparo de imagem (RestoreHealth) finalizado.");
            });

            // 4. DISM StartComponentCleanup (Limpeza da WinSxS)
            AnsiConsole.Status().Start("Executando [cyan]Limpeza de Componentes[/] (WinSxS)...", ctx => 
            {
                ctx.Status("Removendo arquivos de atualizações antigas para liberar espaço...");
                ExecutarComandoSimples("dism /online /cleanup-image /startcomponentcleanup");
                AnsiConsole.MarkupLine("[[[green]OK[/]]] Limpeza da pasta WinSideBySide concluída com sucesso!");
            });

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold green]Processo de manutenção do sistema finalizado![/]");
            }
        }

        public void DiagnosticoRede()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[blue]Diagnóstico de Rede[/]").RuleStyle("grey").Justify(Justify.Left));

            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            var comando = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Selecione o teste de rede:")
                    .AddChoices(new[] {
                        "Teste de Latência (Ping 8.8.8.8)",
                        "Rastrear Rota (Tracert/Traceroute)",
                        "Teste de Gateway Local (Ping Roteador)",
                        "Verificar DNS (NSLookup)",
                        "Informações de Interface (IPConfig/IP A)",
                        "Voltar"
                    }));

            if (comando == "Voltar") return;

            string cmdExecutar = comando switch
            {
                "Teste de Latência (Ping 8.8.8.8)" => 
                    isWindows ? "ping 8.8.8.8 -n 10" : "ping 8.8.8.8 -c 10",

                "Rastrear Rota (Tracert/Traceroute)" => 
                    isWindows ? "tracert 8.8.8.8" : "traceroute 8.8.8.8",

                "Teste de Gateway Local (Ping Roteador)" => 
                    isWindows ? "ping 192.168.1.1 -n 10" : "ping 192.168.1.1 -c 10",

                "Verificar DNS (NSLookup)" => 
                    "nslookup google.com", // O comando nslookup costuma ser universal

                "Informações de Interface (IPConfig/IP A)" => 
                    isWindows ? "ipconfig /all" : "ip a",

                _ => ""
            };

            AnsiConsole.MarkupLine($"\n[yellow]Executando:[/] [white]{cmdExecutar}[/]\n");
            
            // Executa o comando de forma interativa conforme sua infraestrutura
            ExecutarComandoInterativo(cmdExecutar);

            AnsiConsole.MarkupLine("\n[grey]Pressione qualquer tecla para voltar ao menu de redes...[/]");
            Console.ReadKey();
            DiagnosticoRede(); 
        }

        private void ExecutarComandoInterativo(string comando)
        {
            // Define qual "casca" usar: CMD no Windows ou Bash no Linux
            string shell = _isWindows ? "cmd.exe" : "/bin/bash";
            string args = _isWindows ? $"/c {comando}" : $"-c \"{comando}\"";

            ProcessStartInfo psi = new()
            {
                FileName = shell,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = false 
            };
            Process.Start(psi)?.WaitForExit();
        }

    }
}