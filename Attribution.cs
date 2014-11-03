using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;


public class Attribution : MonoBehaviour {
	private static bool isInitialized = false;
	private static bool adjustAttributesSet = false;


	private static string NEW_ACCOUNT_CREATED_KEY = "new_account_created";
	private static string TUTORIAL_COMPLETE_KEY = "tutorial_complete";
	private static string APP_OPENS_002_KEY = "app_opens_002";
	private static string APP_OPENS_020_KEY = "app_opens_020";
	private static string INVITES_FB_KEY = "invites_fb";
	private static string INVITES_SMS_KEY = "invites_sms";
	private static string ADVERTISER_ID = "1835";
	private static string IAP_KEY = "IAP";

	private static string s;

	private static string temp;


	#region singleton implementation
	private static Attribution m_Instance = null;
	
	public static Attribution Instance {
		get {
			if((object)m_Instance != null) {
				return m_Instance;
			}
			// Instance requiered for the first time, we look for it
			else {
				m_Instance = GameObject.FindObjectOfType(typeof(Attribution)) as Attribution;
				
				// Object not found, we create a temporary one
				if( m_Instance == null ) {
					m_Instance = new GameObject(typeof(Attribution).ToString(), typeof(Attribution)).GetComponent<Attribution>();
					
					// Problem during the creation, this should not happen
					if( m_Instance == null ) {
						Debug.LogError("MonoSingleton: Problem during the creation of " + typeof(Attribution).ToString());
					}
				}
			}
			
			return m_Instance;
		}
	}
	#endregion

	#region unity lifecycle
	protected virtual void Awake()
	{
		if( m_Instance == null ) {
			m_Instance = this as Attribution;
			DontDestroyOnLoad(gameObject);
		}
	}
	
	public void Destroy()
	{
		m_Instance = null;
		
		Destroy(gameObject);
	}
	
	protected virtual void OnApplicationQuit()
	{
		m_Instance = null;
	}
	#endregion

	public static void initialize() {
		if(isInitialized) {
			return;
		}
		Config config = Config.getDefaultConfig ();
		initialize (config);
	}

	public static void initialize(Config config) {

		if (config==null || isInitialized || !config.isValid ()) {
			return;
		}
		MATInternalInitialize (config);
		isInitialized = true;
	}

	private static void MATInternalInitialize(Config config) {
		if (isInitialized) {
			return;
		}
		MATBinding.Init (config.matAdvertiserId, config.matConversionKey);

		//MATBinding.SetPublisherReferenceId (ADVERTISER_ID);  TO DO find the ID
//		#if UNITY_ANDROID && !UNITY_EDITOR
//		AndroidJavaClass jc = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
//		AndroidJavaObject currentActivity = jc.GetStatic<AndroidJavaObject> ("currentActivity");
//		AndroidJavaClass client = new AndroidJavaClass ("com.google.android.gms.ads.identifier.AdvertisingIdClient");
//		AndroidJavaObject adInfo = client.CallStatic<AndroidJavaObject> ("getAdvertisingIdInfo",currentActivity);
//		MATBinding.SetGoogleAdvertisingId (adInfo.Call<string> ("getId").ToString (), adInfo.Call<bool> ("isLimitAdTrackingEnabled"));
//
//		Debug.Log ("GOOGLE PLAYER IDDDDDD" + adInfo.Call<string> ("getId").ToString ());
//		#endif

		MATBinding.SetDebugMode (config.debug);
		MATBinding.SetAllowDuplicates (config.debug);
	}

	public static void setUserId(string userId) {
		if (!isInitialized) { //TO DO complain
			return;
		}
		MATBinding.SetUserId (userId);
		// TO DO ADJUSTHELPER USERID
	}

	public static void setUserName(string userName) {
		if (!isInitialized) {
			return;
		}
		MATBinding.SetUserName(userName);
	}

	public static void setUserEmail(string userEmail) {
		if (!isInitialized) {
			return;
		}
		MATBinding.SetUserEmail (userEmail);
	}

	public static void trackNewAccountCreated() {
		if (!isInitialized) {
			return;
		}
		MATBinding.MeasureAction (NEW_ACCOUNT_CREATED_KEY);
	}

	public static void trackTutorialComplete() {
		if(!isInitialized){
			return;
		}
		MATBinding.MeasureAction (TUTORIAL_COMPLETE_KEY);
	}

	public static void trackSecondAppOpen() {
		if (!isInitialized) {
			return;
		}
		MATBinding.MeasureAction(APP_OPENS_002_KEY);
	}

	public static void trackTwentiethAppOpen() {
		if (!isInitialized) {
			return;
		}
		MATBinding.MeasureAction(APP_OPENS_020_KEY);
	}

	public static void trackFacebookInvitation() {
		if (!isInitialized) {
			return;
		}
		MATBinding.MeasureAction(INVITES_FB_KEY);	
	}
	
	public static void trackSmsInvitation() {
		if (!isInitialized) {
			return;
		}
		MATBinding.MeasureAction(INVITES_SMS_KEY);
	}

	public static void trackIap(List<IapLedgerItem>iapLedgerItemList, string currencyCode, string referenceId){
		if (!isInitialized) {
			return;
		}
		int totalInCents = 0;
		List<MATItem> matEventItemList = new List<MATItem> ();
		MATItem[] matEventItemArray;
		foreach (IapLedgerItem iapLedgerItem in iapLedgerItemList) {
			totalInCents += iapLedgerItem.quantity * iapLedgerItem.priceInCents;
			matEventItemList.Add (iapLedgerItem.toEventItem());

		}
		string receiptData = null;
		string receiptSignature = null;
		matEventItemArray = matEventItemList.ToArray ();
		MATBinding.MeasureActionWithEventItems (IAP_KEY,matEventItemArray,matEventItemList.Count,referenceId,((double)totalInCents/100.0d),currencyCode, 1, receiptData, receiptSignature);
	}

	public class Config {
		private static string DEFAULT_PATH = "attribution.config.json";
		
		private static string ADJUST_SET_PROPERTIES_TOKEN_KEY = "adjustSetPropertiesToken";
		private static string ADJUST_SET_USER_ID_TOKEN_KEY = "adjustSetUserIdToken";
		private static string MAT_ADVERTISER_ID_KEY = "matAdvertiserId";
		private static string MAT_CONVERSION_KEY_KEY = "matConversionKey";
		private static string DEBUG_KEY = "debug";
		private static string SANDBOX_KEY = "sandbox";
		
		public string adjustSetPropertiesToken;
		public string adjustSetUserIdToken;
		public string matAdvertiserId;
		public string matConversionKey;
		public bool debug;
		public bool sandbox;

		public static Config getDefaultConfig () {
			try {
				string configJsonString = System.IO.File.ReadAllText (DEFAULT_PATH);
				JSONNode configJsonNode = JSON.Parse(configJsonString);
				string adjustSetPropertiesToken = configJsonNode[ADJUST_SET_PROPERTIES_TOKEN_KEY].Value;
				string adjustSetUserIdToken = configJsonNode[ADJUST_SET_USER_ID_TOKEN_KEY].Value;
				string matAdvertiserId = configJsonNode[MAT_ADVERTISER_ID_KEY].Value;
				string matConversionKey = configJsonNode[MAT_CONVERSION_KEY_KEY].Value;
				bool debug = configJsonNode[DEBUG_KEY].AsBool;
				bool sandbox = configJsonNode[SANDBOX_KEY].AsBool;
				return new Config (adjustSetPropertiesToken, adjustSetUserIdToken, matAdvertiserId, matConversionKey, debug, sandbox);
			}
			catch(System.Exception e) {
				Debug.Log ("Failed to load default config .json. Error: " + e.ToString ());
			}
			return null;
		}

		public Config(
			string adjustSetPropertiesToken,
			string adjustSetUserIdToken,
			string matAdvertiserId,
			string matConversionKey,
			bool debug,
			bool sandbox
		) {
			this.adjustSetPropertiesToken = adjustSetPropertiesToken;
			this.adjustSetUserIdToken = adjustSetUserIdToken;
			this.matAdvertiserId = matAdvertiserId;
			this.matConversionKey = matConversionKey;
			this.debug = debug;
			this.sandbox = sandbox;
		}

		public bool isValid() {
			return adjustSetPropertiesToken != null
				&& adjustSetUserIdToken != null
					&& matAdvertiserId != null
					&& matConversionKey != null;
		}
		
	}
	public class IapLedgerItem {
		public string itemName;
		public string itemSku;
		public int priceInCents;
		public int quantity;

		public IapLedgerItem(string itemName, string itemSku, int priceInCents, int quantity) {
			this.itemName = itemName;
			this.itemSku = itemSku;
			this.priceInCents = priceInCents;
			this.quantity = quantity;
		}

		public MATItem toEventItem() {
			double priceInDollars = ((double)priceInCents) / 100.0d;
			MATItem item = new MATItem ();
			item.name = itemName;
			item.quantity = quantity;
			item.unitPrice = priceInDollars;
			item.revenue = quantity * priceInDollars;
			item.attribute1 = itemSku;
			return item;
		}

	}
	
}	