# FERRAMENTA DE MANUTENÇÃO DE TI

## 📝 Descrição
Ferramenta de linha de comando (CLI) desenvolvida em **.NET** para automação de diagnósticos de hardware e instalação de softwares essenciais (setup) em novas máquinas.

---

## ✨ Funcionalidades Principais
* **Verificação de Hardware:** Monitoramento de SO, CPU, RAM e Disco.
* **Instalação Automatizada:** Menu de seleção múltipla para instalação silenciosa de navegadores, runtimes, compressores e ferramentas de acesso remoto.
* **Diagnóstico de Saúde:** Relatório rápido sobre a integridade do sistema.
* **Otimizações de Sistema:** Automação de comandos de CMD.
* **Interface Moderna:** Relatório rápido sobre a integridade do sistema.

---

## 📂 Estrutura do Projeto
A organização das pastas foi pensada para separar a interface da lógica de negócio:

```
ToolManutencao/
│
├── src/
│   └── ToolManutencao.Console/         # Projeto principal
│       ├── Commands/                   # Lógica dos comandos do menu
│       ├── Services/                   # Lógica de hardware, WMI e Winget
│       ├── Models/                     # Classes de dados (ex: HardwareInfo)
│       ├── Program.cs                  # Ponto de entrada (Main)
│       └── ToolManutencao.csproj       # Arquivo de projeto .NET
│
├── .gitignore                          # Filtro de arquivos para o Git
└── README.md                           # Documentação do projeto
```

---

## 🔍 Explicação dos Diretórios
* **Commands:** Gerencia o que acontece quando o usuário escolhe uma opção no menu. Separa a interface da lógica bruta.
* **Services:** Onde a "mágica" acontece. Contém o código que fala com o Windows para ler sensores ou baixar programas.
* **Models:** Contém as estruturas de dados. É como o sistema entende o que é um "Processador" ou uma "Memória".
* **Program.cs:** O ponto de partida onde o sistema nasce e configura o ambiente de execução.

## ⚙️ Requisitos
* **Windows 10/11** (Para suporte total ao Winget e WMI).
* **.NET SDK 8.0+.**
* **Permissões de Administrador** (Necessário para a instalação de softwares e certas consultas de hardware).

## 🚀 Como Compilar e Rodar
1 - Certifique-se de ter o SDK do .NET instalado.

2 - Abra o terminal na pasta raiz do projeto.

3 - Para rodar diretamente:

```
dotnet run --project src/FerramentaDeManutencao.Console
```

4 - Para gerar o executável final para o Windows:

```
dotnet publish -c Release -r win-x64 --self-contained
```

5 - Para gerar o executável final para o Linux:
```
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
```