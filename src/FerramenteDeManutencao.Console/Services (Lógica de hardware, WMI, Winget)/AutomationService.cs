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
            { "Java Runtime Environment", "Oracle.JavaRuntimeEnvironment" },

            // Office
            { "Microsoft Office 365", "Microsoft.Office" }
        };

        public void InstalarSoftwares()
        {
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

        public void LimparArquivosTemporarios()
        {
            // 1. Obter discos prontos e adicionar opção global
            var drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady)
                .Select(d => d.Name)
                .ToList();
            
            drives.Add("Limpar Todos");

            // Menu de seleção estilizado
            var escolha = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Qual disco deseja limpar com o Cleanmgr?[/]")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Mova para cima e para baixo para selecionar)[/]")
                    .AddChoices(drives));

            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .Start("Limpando arquivos temporários...", ctx =>
                {
                    // --- PARTE 1: Limpeza Manual de Pastas Críticas ---
                    string[] pastas = {
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch"),
                        Path.GetTempPath(), // %temp% do usuário
                        @"C:\Windows\Temp"  // temp do sistema
                    };

                    foreach (var pasta in pastas)
                    {
                        try
                        {
                            ctx.Status($"Acessando: {pasta}");
                            DirectoryInfo di = new DirectoryInfo(pasta);

                            // Deletar arquivos
                            foreach (FileInfo file in di.GetFiles())
                            {
                                try { file.Delete(); } catch { /* Arquivo em uso, ignorar */ }
                            }

                            // Deletar subpastas
                            foreach (DirectoryInfo dir in di.GetDirectories())
                            {
                                try { dir.Delete(true); } catch { /* Pasta em uso, ignorar */ }
                            }
                        }
                        catch (Exception) 
                        { 
                            AnsiConsole.MarkupLine($"[grey]Aviso: Sem permissão total para acessar {pasta}[/]"); 
                        }
                    }

                    // --- PARTE 2: Execução do Cleanmgr conforme escolha ---
                    ctx.Status("[bold blue]Iniciando Cleanmgr...[/]");

                    if (escolha == "Limpar Todos")
                    {
                        // O parâmetro /autoclean executa a limpeza padrão em todos os discos
                        ExecutarComandoSimples("cleanmgr /autoclean");
                    }
                    else
                    {
                        // Extrai a letra do drive (ex: "C:")
                        string driveLetra = escolha.Substring(0, 2);
                        // /sagerun:1 usa configurações pré-definidas (pode exigir /sageset anterior)
                        // Se preferir a limpeza padrão do drive, pode usar apenas cleanmgr /d {driveLetra}
                        ExecutarComandoSimples($"cleanmgr /d {driveLetra} /sagerun:1");
                    }

                    AnsiConsole.MarkupLine("[[[green]OK[/]]] Limpeza de arquivos temporários concluída!");
                });
        }

        public void RepararSistema()
        {
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
}