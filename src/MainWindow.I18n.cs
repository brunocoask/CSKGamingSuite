using System.Collections.Generic;
using System.Windows.Controls;

namespace CSKGamingSuite;

public partial class MainWindow
{
    private class LangOption
    {
        public string Code { get; set; } = "";
        public string Display { get; set; } = "";
        public override string ToString() => Display;
    }

    private bool _langInitializing;

    private void InitLanguageSelector()
    {
        _langInitializing = true;
        try
        {
            CmbLanguage.Items.Clear();
            int selectedIndex = 0;
            int idx = 0;
            foreach (var kvp in LocalizationManager.SupportedLanguages)
            {
                var opt = new LangOption { Code = kvp.Key, Display = kvp.Value };
                CmbLanguage.Items.Add(opt);
                if (kvp.Key == LocalizationManager.CurrentLanguage)
                {
                    selectedIndex = idx;
                }
                idx++;
            }
            CmbLanguage.SelectedIndex = selectedIndex;
        }
        finally
        {
            _langInitializing = false;
        }
    }

    private void CmbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_langInitializing) return;
        if (CmbLanguage.SelectedItem is LangOption opt)
        {
            LocalizationManager.SetLanguage(opt.Code);
            UpdateCurrentPageTitle();
            UpdateDashboard();
            if (PageLauncher.IsVisible) _ = ScanGamesAsync();
            LocalizationManager.TranslateTree(this);
        }
    }

    private void UpdateCurrentPageTitle()
    {
        if (TitlePage == null) return;
        string activeTag = "";
        foreach (var p in new System.Windows.FrameworkElement[] {
            PageDash, PageSystem, PageSensors, PageGpu, PageGames, PageBottleneck,
            PageOverlay, PageBench, PageOptimize, PageOc, PageInteg, PageLauncher,
            PageDriver, PageStartup, PageApps, PageMakerPorts, PageMakerDrv,
            PageDisks, PageEnergy, PageNet, PageStress, PageUltrawide })
        {
            if (p.Visibility == System.Windows.Visibility.Visible)
            {
                activeTag = p.Name;
                break;
            }
        }

        TitlePage.Text = activeTag switch
        {
            "PageDash" => LocalizationManager.GetString("Nav_Overview", "Visão Geral"),
            "PageSystem" => LocalizationManager.GetString("Nav_System", "Sistema"),
            "PageSensors" => LocalizationManager.GetString("Nav_Sensors", "Sensores"),
            "PageGpu" => LocalizationManager.GetString("Nav_Gpu", "Placa de Vídeo"),
            "PageDisks" => LocalizationManager.GetString("Nav_Disks", "Saúde dos Discos"),
            "PageEnergy" => LocalizationManager.GetString("Nav_Energy", "Energia"),
            "PageNet" => LocalizationManager.GetString("Nav_Net", "Rede"),
            "PageLauncher" => LocalizationManager.GetString("Nav_Launcher", "Biblioteca"),
            "PageGames" => LocalizationManager.GetString("Nav_Games", "FPS Estimado"),
            "PageBottleneck" => LocalizationManager.GetString("Nav_Bottleneck", "Calculadora de Gargalo"),
            "PageOverlay" => LocalizationManager.GetString("Nav_Overlay", "Overlay FPS"),
            "PageBench" => LocalizationManager.GetString("Nav_Bench", "Benchmark"),
            "PageStress" => LocalizationManager.GetString("Nav_Stress", "Teste de Estresse"),
            "PageOc" => LocalizationManager.GetString("Nav_Oc", "Overclock Seguro"),
            "PageOptimize" => LocalizationManager.GetString("Nav_Optimize", "Otimização"),
            "PageStartup" => LocalizationManager.GetString("Nav_Startup", "Inicialização"),
            "PageApps" => LocalizationManager.GetString("Nav_Apps", "Instalar Programas"),
            "PageDriver" => LocalizationManager.GetString("Nav_Driver", "Drivers"),
            "PageInteg" => LocalizationManager.GetString("Nav_Integ", "Sincronização RGB"),
            "PageUltrawide" => LocalizationManager.GetString("Nav_Ultrawide", "Gerenciador Ultrawide 34\""),
            "PageMakerPorts" => LocalizationManager.GetString("Nav_MakerPorts", "Portas COM"),
            "PageMakerDrv" => LocalizationManager.GetString("Nav_MakerDrv", "Drivers Seriais"),
            _ => ""
        };
    }
}
