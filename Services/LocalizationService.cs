using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BluetoothAudioReceiver.Services;

/// <summary>
/// Localization service supporting multiple languages.
/// Refactored to use lazy loading for translation dictionaries to reduce startup time and memory usage.
/// </summary>
public class LocalizationService : INotifyPropertyChanged
{
    private static LocalizationService? _instance;
    public static LocalizationService Instance => _instance ??= new LocalizationService();
    
    private string _currentLanguage = "en";
    private Dictionary<string, string> _cachedTranslation;

    // Cache for loaded translations to avoid re-allocating
    private readonly Dictionary<string, Dictionary<string, string>> _loadedTranslations = new();
    private readonly object _lock = new();
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public LocalizationService()
    {
        // Initialize with default language (English)
        _cachedTranslation = GetTranslation("en") ?? new Dictionary<string, string>();
    }

    public static readonly Dictionary<string, string> AvailableLanguages = new()
    {
        { "en", "English" },
        { "de", "Deutsch" },
        { "es", "Español" },
        { "fr", "Français" },
        { "it", "Italiano" },
        { "pt", "Português" },
        { "nl", "Nederlands" },
        { "pl", "Polski" },
        { "ru", "Русский" },
        { "uk", "Українська" },
        { "tr", "Türkçe" },
        { "ja", "日本語" },
        { "ko", "한국어" },
        { "zh", "中文" },
        { "ar", "العربية" }
    };
    
    // Helper to get translation dictionary (cached or loaded on demand)
    private Dictionary<string, string>? GetTranslation(string languageCode)
    {
        lock (_lock)
        {
            if (_loadedTranslations.TryGetValue(languageCode, out var dict))
            {
                return dict;
            }

            dict = LoadTranslation(languageCode);
            if (dict != null)
            {
                _loadedTranslations[languageCode] = dict;
            }
            return dict;
        }
    }

    // Loads translation data on demand
    private Dictionary<string, string>? LoadTranslation(string languageCode)
    {
        return languageCode switch
        {
            "en" => new Dictionary<string, string>
            {
                ["AppTitle"] = "Audio Receiver",
                ["Show"] = "Show",
                ["Exit"] = "Exit",
                ["Idle"] = "Idle",
                ["Connecting"] = "Connecting...",
                ["Connected"] = "Connected",
                ["Streaming"] = "Streaming",
                ["UnknownDevice"] = "Unknown Device",
                ["Scanning"] = "Scanning for devices...",
                ["DevicesFound"] = "{0} device(s) found",
                ["AudioReadyFrom"] = "Audio ready from {0}",
                ["ReceivingAudioFrom"] = "Receiving audio from {0}",
                ["OpeningConnectionTo"] = "Opening connection to {0}",
                ["SelectDevice"] = "Select a device to connect",
                ["NoDeviceSelected"] = "No device selected",
                ["SelectFromList"] = "Select a device from the list",
                ["Devices"] = "DEVICES",
                ["Refresh"] = "Refresh",
                ["NoDevicesFound"] = "No devices found.\nPair your phone in Windows settings.",
                ["Connect"] = "Connect",
                ["Disconnect"] = "Disconnect",
                ["Settings"] = "Settings",
                ["Help"] = "Help & Troubleshooting",
                ["Volume"] = "VOLUME",
                ["Autostart"] = "AUTOSTART",
                ["StartWithWindows"] = "Start with Windows",
                ["StartMinimized"] = "Start minimized",
                ["Behavior"] = "BEHAVIOR",
                ["MinimizeToTray"] = "Minimize to system tray",
                ["AutoConnect"] = "Auto-connect",
                ["ShowNotifications"] = "Show notifications",
                ["Language"] = "LANGUAGE",
                ["Save"] = "Save",
                ["Cancel"] = "Cancel",
                ["Minimized"] = "Minimized",
                ["AppRunningInTray"] = "App is running in the system tray.",
                ["AudioConnectionTo"] = "Audio connection to",
                ["Established"] = "established.",
                ["QuickStart"] = "QUICK START",
                ["QuickStartSteps"] = "1. Pair phone in Windows Bluetooth settings\n2. Start app and select device from list\n3. Click \"Connect\"\n4. Play audio on phone",
                ["KnownIssues"] = "⚠️ KNOWN ISSUES",
                ["NetworkSlowdown"] = "Network/Download speed drops",
                ["NetworkSlowdownDesc"] = "Bluetooth and WiFi often share the same 2.4 GHz frequency band, causing interference.",
                ["Solution"] = "Solution:",
                ["NetworkSolution"] = "• Use 5 GHz WiFi (best option!)\n• Device Manager → WiFi adapter → Properties\n• Disable \"Bluetooth Collaboration\"\n• Use separate USB Bluetooth dongle",
                ["NoDevicesIssue"] = "No devices in list",
                ["NoDevicesSolution"] = "• Pair phone with PC in Windows settings\n• Is Bluetooth enabled on phone and PC?\n• Windows 10 Version 2004+ or Windows 11 required",
                ["ConnectionDrops"] = "Connection drops",
                ["ConnectionDropsSolution"] = "• Update Bluetooth drivers\n• Device Manager → Bluetooth adapter → Disable power saving\n• Keep phone in range (<10m)",
                ["SystemRequirements"] = "SYSTEM REQUIREMENTS",
                ["Requirements"] = "• Windows 10 Version 2004 (May 2020 Update) or newer\n• Windows 11 (all versions)\n• Bluetooth adapter with A2DP support\n• Paired Bluetooth device (phone, tablet, etc.)",
                ["Info"] = "INFO",
                ["Version"] = "Version:",
                ["DevelopedWith"] = "Developed with .NET 8 and WPF"
            },
            "de" => new Dictionary<string, string>
            {
                ["AppTitle"] = "Audio Receiver",
                ["Show"] = "Anzeigen",
                ["Exit"] = "Beenden",
                ["Idle"] = "Bereit",
                ["Connecting"] = "Verbinden...",
                ["Connected"] = "Verbunden",
                ["Streaming"] = "Streaming",
                ["UnknownDevice"] = "Unbekanntes Gerät",
                ["Scanning"] = "Suche nach Geräten...",
                ["DevicesFound"] = "{0} Gerät(e) gefunden",
                ["AudioReadyFrom"] = "Audio bereit von {0}",
                ["ReceivingAudioFrom"] = "Empfange Audio von {0}",
                ["OpeningConnectionTo"] = "Verbinde mit {0}",
                ["SelectDevice"] = "Wähle ein Gerät zum Verbinden",
                ["NoDeviceSelected"] = "Kein Gerät ausgewählt",
                ["SelectFromList"] = "Wähle ein Gerät aus der Liste",
                ["Devices"] = "GERÄTE",
                ["Refresh"] = "Aktualisieren",
                ["NoDevicesFound"] = "Keine Geräte gefunden.\nKopple dein Handy in den Windows-Einstellungen.",
                ["Connect"] = "Verbinden",
                ["Disconnect"] = "Trennen",
                ["Settings"] = "Einstellungen",
                ["Help"] = "Hilfe & Problemlösung",
                ["Volume"] = "LAUTSTÄRKE",
                ["Autostart"] = "AUTOSTART",
                ["StartWithWindows"] = "Mit Windows starten",
                ["StartMinimized"] = "Minimiert starten",
                ["Behavior"] = "VERHALTEN",
                ["MinimizeToTray"] = "In System Tray minimieren",
                ["AutoConnect"] = "Automatisch verbinden",
                ["ShowNotifications"] = "Benachrichtigungen anzeigen",
                ["Language"] = "SPRACHE",
                ["Save"] = "Speichern",
                ["Cancel"] = "Abbrechen",
                ["Minimized"] = "Minimiert",
                ["AppRunningInTray"] = "Die App läuft weiter im System Tray.",
                ["AudioConnectionTo"] = "Audio-Verbindung zu",
                ["Established"] = "hergestellt.",
                ["QuickStart"] = "SCHNELLSTART",
                ["QuickStartSteps"] = "1. Handy in Windows Bluetooth-Einstellungen koppeln\n2. App starten und Gerät aus der Liste wählen\n3. \"Verbinden\" klicken\n4. Audio auf dem Handy abspielen",
                ["KnownIssues"] = "⚠️ BEKANNTE PROBLEME",
                ["NetworkSlowdown"] = "Netzwerk/Download-Geschwindigkeit sinkt",
                ["NetworkSlowdownDesc"] = "Bluetooth und WiFi nutzen oft das gleiche 2.4 GHz Frequenzband.",
                ["Solution"] = "Lösung:",
                ["NetworkSolution"] = "• 5 GHz WiFi nutzen (beste Option!)\n• Geräte-Manager → WiFi-Adapter → Eigenschaften\n• \"Bluetooth Collaboration\" deaktivieren\n• Separaten USB-Bluetooth-Dongle verwenden",
                ["NoDevicesIssue"] = "Keine Geräte in der Liste",
                ["NoDevicesSolution"] = "• Handy mit PC in Windows-Einstellungen koppeln\n• Bluetooth auf Handy und PC aktiviert?\n• Windows 10 Version 2004+ oder Windows 11 nötig",
                ["ConnectionDrops"] = "Verbindung bricht ab",
                ["ConnectionDropsSolution"] = "• Bluetooth-Treiber aktualisieren\n• Geräte-Manager → Bluetooth-Adapter → Energiesparmodus deaktivieren\n• Handy in Reichweite halten (<10m)",
                ["SystemRequirements"] = "SYSTEMANFORDERUNGEN",
                ["Requirements"] = "• Windows 10 Version 2004 (Mai 2020 Update) oder neuer\n• Windows 11 (alle Versionen)\n• Bluetooth-Adapter mit A2DP Unterstützung\n• Gekoppeltes Bluetooth-Gerät (Handy, Tablet, etc.)",
                ["Info"] = "INFO",
                ["Version"] = "Version:",
                ["DevelopedWith"] = "Entwickelt mit .NET 8 und WPF"
            },
            "es" => new Dictionary<string, string>
            {
                ["AppTitle"] = "Audio Receiver",
                ["Idle"] = "Inactivo",
                ["Connecting"] = "Conectando...",
                ["Connected"] = "Conectado",
                ["Streaming"] = "Transmitiendo",
                ["SelectDevice"] = "Selecciona un dispositivo para conectar",
                ["NoDeviceSelected"] = "Ningún dispositivo seleccionado",
                ["SelectFromList"] = "Selecciona un dispositivo de la lista",
                ["Devices"] = "DISPOSITIVOS",
                ["Refresh"] = "Actualizar",
                ["NoDevicesFound"] = "No se encontraron dispositivos.\nEmpareja tu teléfono en la configuración de Windows.",
                ["Connect"] = "Conectar",
                ["Disconnect"] = "Desconectar",
                ["Settings"] = "Configuración",
                ["Help"] = "Ayuda y Solución de problemas",
                ["Volume"] = "VOLUMEN",
                ["Autostart"] = "INICIO AUTOMÁTICO",
                ["StartWithWindows"] = "Iniciar con Windows",
                ["StartMinimized"] = "Iniciar minimizado",
                ["Behavior"] = "COMPORTAMIENTO",
                ["MinimizeToTray"] = "Minimizar a la bandeja del sistema",
                ["AutoConnect"] = "Conectar automáticamente",
                ["ShowNotifications"] = "Mostrar notificaciones",
                ["Language"] = "IDIOMA",
                ["Save"] = "Guardar",
                ["Cancel"] = "Cancelar",
                ["Minimized"] = "Minimizado",
                ["AppRunningInTray"] = "La aplicación se ejecuta en la bandeja del sistema.",
                ["AudioConnectionTo"] = "Conexión de audio a",
                ["Established"] = "establecida."
            },
            "fr" => new Dictionary<string, string>
            {
                ["AppTitle"] = "Audio Receiver",
                ["Idle"] = "Inactif",
                ["Connecting"] = "Connexion...",
                ["Connected"] = "Connecté",
                ["Streaming"] = "Diffusion",
                ["SelectDevice"] = "Sélectionnez un appareil pour vous connecter",
                ["NoDeviceSelected"] = "Aucun appareil sélectionné",
                ["SelectFromList"] = "Sélectionnez un appareil dans la liste",
                ["Devices"] = "APPAREILS",
                ["Refresh"] = "Actualiser",
                ["NoDevicesFound"] = "Aucun appareil trouvé.\nAssociez votre téléphone dans les paramètres Windows.",
                ["Connect"] = "Connecter",
                ["Disconnect"] = "Déconnecter",
                ["Settings"] = "Paramètres",
                ["Help"] = "Aide et Dépannage",
                ["Volume"] = "VOLUME",
                ["Autostart"] = "DÉMARRAGE AUTO",
                ["StartWithWindows"] = "Démarrer avec Windows",
                ["StartMinimized"] = "Démarrer minimisé",
                ["Behavior"] = "COMPORTEMENT",
                ["MinimizeToTray"] = "Minimiser dans la barre système",
                ["AutoConnect"] = "Connexion automatique",
                ["ShowNotifications"] = "Afficher les notifications",
                ["Language"] = "LANGUE",
                ["Save"] = "Enregistrer",
                ["Cancel"] = "Annuler"
            },
            "it" => new Dictionary<string, string>
            {
                ["AppTitle"] = "Audio Receiver",
                ["Idle"] = "Inattivo",
                ["Connecting"] = "Connessione...",
                ["Connected"] = "Connesso",
                ["Streaming"] = "Streaming",
                ["SelectDevice"] = "Seleziona un dispositivo da connettere",
                ["Devices"] = "DISPOSITIVI",
                ["Connect"] = "Connetti",
                ["Disconnect"] = "Disconnetti",
                ["Settings"] = "Impostazioni",
                ["Volume"] = "VOLUME",
                ["Language"] = "LINGUA",
                ["Save"] = "Salva",
                ["Cancel"] = "Annulla"
            },
            "pt" => new Dictionary<string, string>
            {
                ["AppTitle"] = "Audio Receiver",
                ["Idle"] = "Inativo",
                ["Connecting"] = "Conectando...",
                ["Connected"] = "Conectado",
                ["Streaming"] = "Transmitindo",
                ["SelectDevice"] = "Selecione um dispositivo para conectar",
                ["Devices"] = "DISPOSITIVOS",
                ["Connect"] = "Conectar",
                ["Disconnect"] = "Desconectar",
                ["Settings"] = "Configurações",
                ["Volume"] = "VOLUME",
                ["Language"] = "IDIOMA",
                ["Save"] = "Salvar",
                ["Cancel"] = "Cancelar"
            },
            "nl" => new Dictionary<string, string>
            {
                ["AppTitle"] = "Audio Receiver",
                ["Idle"] = "Inactief",
                ["Connecting"] = "Verbinden...",
                ["Connected"] = "Verbonden",
                ["Streaming"] = "Streaming",
                ["Devices"] = "APPARATEN",
                ["Connect"] = "Verbinden",
                ["Disconnect"] = "Verbreken",
                ["Settings"] = "Instellingen",
                ["Volume"] = "VOLUME",
                ["Language"] = "TAAL",
                ["Save"] = "Opslaan",
                ["Cancel"] = "Annuleren"
            },
            "pl" => new Dictionary<string, string>
            {
                ["AppTitle"] = "Audio Receiver",
                ["Idle"] = "Bezczynny",
                ["Connecting"] = "Łączenie...",
                ["Connected"] = "Połączono",
                ["Streaming"] = "Strumieniowanie",
                ["Devices"] = "URZĄDZENIA",
                ["Connect"] = "Połącz",
                ["Disconnect"] = "Rozłącz",
                ["Settings"] = "Ustawienia",
                ["Volume"] = "GŁOŚNOŚĆ",
                ["Language"] = "JĘZYK",
                ["Save"] = "Zapisz",
                ["Cancel"] = "Anuluj"
            },
            "ru" => new Dictionary<string, string>
            {
                ["AppTitle"] = "Audio Receiver",
                ["Idle"] = "Ожидание",
                ["Connecting"] = "Подключение...",
                ["Connected"] = "Подключено",
                ["Streaming"] = "Воспроизведение",
                ["Devices"] = "УСТРОЙСТВА",
                ["Connect"] = "Подключить",
                ["Disconnect"] = "Отключить",
                ["Settings"] = "Настройки",
                ["Volume"] = "ГРОМКОСТЬ",
                ["Language"] = "ЯЗЫК",
                ["Save"] = "Сохранить",
                ["Cancel"] = "Отмена"
            },
            "uk" => new Dictionary<string, string>
            {
                ["AppTitle"] = "Audio Receiver",
                ["Idle"] = "Очікування",
                ["Connecting"] = "Підключення...",
                ["Connected"] = "Підключено",
                ["Streaming"] = "Відтворення",
                ["Devices"] = "ПРИСТРОЇ",
                ["Connect"] = "Підключити",
                ["Disconnect"] = "Відключити",
                ["Settings"] = "Налаштування",
                ["Volume"] = "ГУЧНІСТЬ",
                ["Language"] = "МОВА",
                ["Save"] = "Зберегти",
                ["Cancel"] = "Скасувати"
            },
            "tr" => new Dictionary<string, string>
            {
                ["AppTitle"] = "Audio Receiver",
                ["Idle"] = "Boşta",
                ["Connecting"] = "Bağlanıyor...",
                ["Connected"] = "Bağlandı",
                ["Streaming"] = "Akış",
                ["Devices"] = "CİHAZLAR",
                ["Connect"] = "Bağlan",
                ["Disconnect"] = "Bağlantıyı Kes",
                ["Settings"] = "Ayarlar",
                ["Volume"] = "SES",
                ["Language"] = "DİL",
                ["Save"] = "Kaydet",
                ["Cancel"] = "İptal"
            },
            "ja" => new Dictionary<string, string>
            {
                ["AppTitle"] = "Audio Receiver",
                ["Idle"] = "待機中",
                ["Connecting"] = "接続中...",
                ["Connected"] = "接続済み",
                ["Streaming"] = "ストリーミング",
                ["Devices"] = "デバイス",
                ["Connect"] = "接続",
                ["Disconnect"] = "切断",
                ["Settings"] = "設定",
                ["Volume"] = "音量",
                ["Language"] = "言語",
                ["Save"] = "保存",
                ["Cancel"] = "キャンセル"
            },
            "ko" => new Dictionary<string, string>
            {
                ["AppTitle"] = "Audio Receiver",
                ["Idle"] = "대기 중",
                ["Connecting"] = "연결 중...",
                ["Connected"] = "연결됨",
                ["Streaming"] = "스트리밍",
                ["Devices"] = "장치",
                ["Connect"] = "연결",
                ["Disconnect"] = "연결 해제",
                ["Settings"] = "설정",
                ["Volume"] = "볼륨",
                ["Language"] = "언어",
                ["Save"] = "저장",
                ["Cancel"] = "취소"
            },
            "zh" => new Dictionary<string, string>
            {
                ["AppTitle"] = "Audio Receiver",
                ["Idle"] = "空闲",
                ["Connecting"] = "连接中...",
                ["Connected"] = "已连接",
                ["Streaming"] = "正在播放",
                ["Devices"] = "设备",
                ["Connect"] = "连接",
                ["Disconnect"] = "断开",
                ["Settings"] = "设置",
                ["Volume"] = "音量",
                ["Language"] = "语言",
                ["Save"] = "保存",
                ["Cancel"] = "取消"
            },
            "ar" => new Dictionary<string, string>
            {
                ["AppTitle"] = "Audio Receiver",
                ["Idle"] = "خامل",
                ["Connecting"] = "جاري الاتصال...",
                ["Connected"] = "متصل",
                ["Streaming"] = "بث",
                ["Devices"] = "الأجهزة",
                ["Connect"] = "اتصال",
                ["Disconnect"] = "قطع الاتصال",
                ["Settings"] = "الإعدادات",
                ["Volume"] = "الصوت",
                ["Language"] = "اللغة",
                ["Save"] = "حفظ",
                ["Cancel"] = "إلغاء"
            },
            _ => null
        };
    }
    
    public string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_currentLanguage != value && AvailableLanguages.ContainsKey(value))
            {
                _currentLanguage = value;

                // Load the new language
                var dict = GetTranslation(value);
                if (dict != null)
                {
                    _cachedTranslation = dict;
                }
                else
                {
                    // Fallback to English if load failed (shouldn't happen for known languages)
                    _cachedTranslation = GetTranslation("en") ?? new Dictionary<string, string>();
                }

                OnPropertyChanged();
                // Notify that all indexed properties may have changed
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            }
        }
    }
    
    public string this[string key]
    {
        get
        {
            // Try cached translation (current language)
            if (_cachedTranslation.TryGetValue(key, out var value))
            {
                return value;
            }
            
            // Fallback to English (only if current is not English)
            if (_currentLanguage != "en")
            {
                var enDict = GetTranslation("en");
                if (enDict != null && enDict.TryGetValue(key, out var enValue))
                {
                    return enValue;
                }
            }
            
            // Return key if not found
            return key;
        }
    }
    
    public string Get(string key) => this[key];
    
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
