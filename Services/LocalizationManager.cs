using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DeliveryApp.Services;

public class LocalizationManager : INotifyPropertyChanged
{
    private static readonly LocalizationManager _instance = new LocalizationManager();
    public static LocalizationManager Instance => _instance;

    private bool _isTamil;
    public bool IsTamil
    {
        get => _isTamil;
        set
        {
            if (_isTamil != value)
            {
                _isTamil = value;
                Preferences.Set("IsTamil", value);
                OnPropertyChanged(null); // Notify all properties changed
            }
        }
    }

    private LocalizationManager()
    {
        _isTamil = Preferences.Get("IsTamil", false);
    }

    public string Translate(string key)
    {
        if (_isTamil && TamilDictionary.TryGetValue(key, out string tamilValue))
        {
            return tamilValue;
        }
        
        if (EnglishDictionary.TryGetValue(key, out string englishValue))
        {
            return englishValue;
        }
        return key;
    }

    public string this[string key] => Translate(key);

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private readonly Dictionary<string, string> EnglishDictionary = new()
    {
        { "AppTitle", "Shift Delivery" },
        { "Login", "Login" },
        { "PhonePlaceholder", "Enter Phone Number" },
        { "PasswordPlaceholder", "Enter Password" },
        { "DashboardTitle", "My Routes" },
        { "TodaysDeliveries", "Today's Deliveries" },
        { "CompleteRoutesSafely", "Complete your routes safely" },
        { "Assigned", "Assigned" },
        { "ReachedLocation", "Reached Location" },
        { "AllCaughtUp", "All Caught Up!" },
        { "NoActiveOrders", "There are no active orders assigned to you right now. Take a break!" },
        { "Refresh", "Refresh" },
        { "Logout", "Logout" },
        { "LogoutConfirm", "Are you sure you want to log out?" },
        { "Yes", "Yes" },
        { "No", "No" },
        { "TakePhoto", "Take Photo of Delivery" },
        { "SubmitDelivery", "Submit Delivery" },
        { "Customer", "Customer" },
        { "Items", "Items" },
        { "Location", "Location" }
    };

    private readonly Dictionary<string, string> TamilDictionary = new()
    {
        { "AppTitle", "ஷிப்ட் டெலிவரி" },
        { "Login", "உள்நுழைக" },
        { "PhonePlaceholder", "தொலைபேசி எண்ணை உள்ளிடவும்" },
        { "PasswordPlaceholder", "கடவுச்சொல்லை உள்ளிடவும்" },
        { "DashboardTitle", "என் வழிகள்" },
        { "TodaysDeliveries", "இன்றைய விநியோகங்கள்" },
        { "CompleteRoutesSafely", "உங்கள் வழிகளை பாதுகாப்பாக முடிக்கவும்" },
        { "Assigned", "ஒதுக்கப்பட்டது" },
        { "ReachedLocation", "இடத்தை அடைந்துவிட்டேன்" },
        { "AllCaughtUp", "எல்லாம் முடிந்தது!" },
        { "NoActiveOrders", "தற்போது உங்களுக்கு எந்த ஆர்டரும் ஒதுக்கப்படவில்லை. ஓய்வெடுங்கள்!" },
        { "Refresh", "புதுப்பிக்கவும்" },
        { "Logout", "வெளியேறு" },
        { "LogoutConfirm", "நீங்கள் நிச்சயமாக வெளியேற வேண்டுமா?" },
        { "Yes", "ஆம்" },
        { "No", "இல்லை" },
        { "TakePhoto", "டெலிவரியை புகைப்படம் எடுக்கவும்" },
        { "SubmitDelivery", "டெலிவரியை சமர்ப்பிக்கவும்" },
        { "Customer", "வாடிக்கையாளர்" },
        { "Items", "பொருட்கள்" },
        { "Location", "இடம்" }
    };
}
