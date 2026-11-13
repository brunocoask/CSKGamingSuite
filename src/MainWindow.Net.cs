using System.Windows;
using System.Windows.Threading;

namespace CSKGamingSuite;

/// <summary>Páginas Energia (bateria e fonte) e Rede (diagnóstico de lag).</summary>
public partial class MainWindow
{
    // ================= Energia =================

    private bool _energyBusy;

    private async void RefreshEnergy()
    {
        if (_energyBusy) return;
        _energyBusy = true;
        BtnEnergyRefresh.IsEnabled = false;
        EnergyStatus.Text = "Lendo bateria e energia…";
        try
        {
            var sensors = _monitor.Rows.ToList();
            var cards = await Task.Run(() => BatteryInfo.Read(sensors));
            EnergyCards.ItemsSource = cards;
            bool hasBattery = cards.Count > 1;
            EnergyStatus.Text = (hasBattery ? "Bateria detectada" : "Nenhuma bateria (PC de mesa)") + $" · lido às {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex) { EnergyStatus.Text = "Falhou: " + ex.Message; }
        finally
        {
            _energyBusy = false;
            BtnEnergyRefresh.IsEnabled = true;
        }
    }

    private void BtnEnergyRefresh_Click(object sender, RoutedEventArgs e) => RefreshEnergy();

    // ================= Rede =================

    private CancellationTokenSource? _netCts;
    private bool _netRunning;
    private List<DiskHealthCard>? _lastNet;

    private async void BtnNetTest_Click(object sender, RoutedEventArgs e)
    {
        if (_netRunning) { _netCts?.Cancel(); return; }
        if (AnyBenchRunning || TestRunning) { NetStatus.Text = "Termine o outro teste antes."; return; }

        _netRunning = true;
        _netCts = new CancellationTokenSource();
        var ct = _netCts.Token;
        BtnNetTest.Content = "CANCELAR";
        NetCards.ItemsSource = null;
        NetProgress.ScaleX = 0;
        try
        {
            var cards = await NetworkTest.Run((text, frac) => Dispatcher.BeginInvoke(new Action(() =>
            {
                NetStatus.Text = text;
                NetProgress.ScaleX = Math.Clamp(frac, 0, 1);
            })), ct);
            _lastNet = cards;
            NetCards.ItemsSource = cards;
            NetStatus.Text = $"Concluído às {DateTime.Now:HH:mm:ss}. O resultado entra no relatório exportado.";
            NetProgress.ScaleX = 1;
        }
        catch (OperationCanceledException) { NetStatus.Text = "Teste cancelado."; NetProgress.ScaleX = 0; }
        catch (Exception ex) { NetStatus.Text = "Falhou: " + ex.Message; NetProgress.ScaleX = 0; }
        finally
        {
            _netRunning = false;
            _netCts?.Dispose();
            _netCts = null;
            BtnNetTest.Content = "TESTAR";
        }
    }

    // ================= Skins & Tema (escuro / claro) =================

    private bool _skinInitializing;

    private void InitSkinSelector()
    {
        _skinInitializing = true;
        try
        {
            CmbSkin.ItemsSource = ThemeManager.Skins;
            CmbSkin.SelectedItem = ThemeManager.CurrentSkin;
        }
        finally
        {
            _skinInitializing = false;
        }
    }

    private void CmbSkin_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_skinInitializing) return;
        if (CmbSkin.SelectedItem is SkinDefinition skin)
        {
            ThemeManager.Apply(skin.Id, _settings.LightTheme);
            _settings.Skin = skin.Id;
            try { _settings.Save(); } catch { }
            UpdateThemeButton();
            if (PageDash.IsVisible) UpdateDashboard();
        }
    }

    private void Theme_Click(object sender, RoutedEventArgs e)
    {
        ThemeManager.Apply(!ThemeManager.IsLight);
        _settings.LightTheme = ThemeManager.IsLight;
        try { _settings.Save(); } catch { }
        UpdateThemeButton();
        if (PageDash.IsVisible) UpdateDashboard();
    }

    /// <summary>Atualiza o botão claro/escuro, os botões de janela da skin ativa e o logo.</summary>
    private void UpdateThemeButton()
    {
        var skin = ThemeManager.CurrentSkin;
        bool light = ThemeManager.IsLight;

        BtnTheme.Visibility = skin.SupportsLightDark ? Visibility.Visible : Visibility.Collapsed;
        BtnTheme.Content = light ? "\uE708" : "\uE706";
        BtnTheme.ToolTip = light ? "Modo escuro" : "Modo claro";

        WindowTrafficLights.Visibility = skin.UseTrafficLights ? Visibility.Visible : Visibility.Collapsed;
        WindowClassicButtons.Visibility = skin.UseTrafficLights ? Visibility.Collapsed : Visibility.Visible;

        Sidebar.Margin = skin.DockedSidebar ? new Thickness(0) : new Thickness(14, 0, 0, 14);
        Sidebar.BorderThickness = skin.DockedSidebar ? new Thickness(0, 0, 1, 0) : new Thickness(1);

        bool hasBadge = !string.IsNullOrEmpty(skin.BadgeText);
        SkinBadge.Visibility = hasBadge ? Visibility.Visible : Visibility.Collapsed;
        SkinBadgeText.Text = skin.BadgeText;

        DashAfterburnerDeck.Visibility = skin.ShowAfterburnerDeck ? Visibility.Visible : Visibility.Collapsed;
        DashBentoLayout.Visibility = skin.ShowBentoNoirDeck ? Visibility.Visible : Visibility.Collapsed;
        DashClassicLayout.Visibility = skin.ShowBentoNoirDeck ? Visibility.Collapsed : Visibility.Visible;

        TitleLogo.Source = new System.Windows.Media.Imaging.BitmapImage(
            new Uri(light ? "pack://application:,,,/Assets/logo-dark.png" : "pack://application:,,,/Assets/logo.png"));
    }
}
