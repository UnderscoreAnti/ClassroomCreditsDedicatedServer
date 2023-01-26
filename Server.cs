using System.Security;
using Godot;
using Godot.Collections;

public partial class Server : Control
{
	private string SaveFilePath = "user://ClassData.json";

	private string CurrentClassName;
	private Array<Dictionary<string, string>> CurrentClass;
	private Dictionary<string, Array<Dictionary<string, string>>> AllClassesData = new();

	private int ServerPort = 9999;
	private int MaxConnections = 64;

	private int AdminID;

	public override void _EnterTree()
	{
		// Guarantees a save file is present at run time
		WriteDataToFile();
	}
	
	
	public override void _Ready()
	{
		InitializeNetwork();
	}
	
	public void InitializeNetwork()
	{
		ENetMultiplayerPeer Peer = new();

		Multiplayer.PeerConnected += ClientConnected;
		Multiplayer.PeerDisconnected += ClientDisconnected;

		Peer.CreateServer(ServerPort, MaxConnections);

		Multiplayer.MultiplayerPeer = Peer;
		GD.Print("Network Connected!");
	}


	public void ClientConnected(long ClientID)
	{
		GD.Print($"{ClientID.ToString()} Connected to the server.");
		if (CurrentClass != new Array<Dictionary<string, string>>())
		{
			RpcId(ClientID, nameof(ConfirmServerConnection), "Hello Moto");
		}

		else
		{
			RpcId(ClientID, nameof(GetCurrentClassData), CurrentClass);
		}
		
	}

	public void ClientDisconnected(long ClientID)
	{
		GD.Print($"{ClientID.ToString()} Disconnected from the server.");
	}

	public void SaveStudentData()
	{
		AllClassesData[CurrentClassName] = CurrentClass;
	}

	public void ChangeClassName(string NewName)
	{
		AllClassesData.Remove(CurrentClassName);
		AllClassesData.Add(NewName, CurrentClass);
	}

	public void AddStudentToCurrentClass(string StudentName)
	{
		Dictionary<string, string> NewStudent = new() {{StudentName, "0000"}};
		CurrentClass.Add(NewStudent);

		foreach (var Key in CurrentClass)
		{
			foreach (var k in Key)
			{
				GD.Print($"Student: {k}");
			}
		}
	}
	

	public void WriteDataToFile(Variant Data = new())
	{
		string Content = JSON.Stringify(Data);
		var File = FileAccess.Open(SaveFilePath, FileAccess.ModeFlags.Write);
		File.StoreString(Content);
		
		File.Dispose();
	}

	public void SendCurrentClassData()
	{
		Rpc(nameof(GetCurrentClassData), CurrentClass);
	}
	
	[RPC(MultiplayerAPI.RPCMode.AnyPeer)]
	public void AddNewClassToDatabase()
	{
		string TempClassName;
			
		Array<Dictionary<string, string>> NewClass = new Array<Dictionary<string, string>>();
		
		if (AllClassesData == new Dictionary<string, Array<Dictionary<string, string>>>())
		{
			TempClassName = "Class #0";
		}

		else
		{
			TempClassName = $"Class #{AllClassesData.Count.ToString()}";
		}
		
		CurrentClassName = TempClassName;
		AllClassesData.Add(TempClassName, NewClass);
		CurrentClass = AllClassesData[TempClassName];

		GD.Print(CurrentClassName);
		Rpc(nameof(UpdateClassName), CurrentClassName);
	}
	
	[RPC(MultiplayerAPI.RPCMode.AnyPeer)]
	public void GetCurrentClassData(string selectedClass)
	{
		int ClientID = Multiplayer.GetRemoteSenderId();
		//RpcId(ClientID, nameof(ProcessClassData), AllClassesData[selectedClass]);
	}
	
	[RPC(MultiplayerAPI.RPCMode.AnyPeer)]
	public void GetAdminData()
	{
		AdminID = Multiplayer.GetRemoteSenderId();
		GD.Print($"Admin Secured! Admin ID: {AdminID.ToString()}".ToString());
	}
	
	[RPC]
	public void RequestAddNewStudent(Dictionary<string, string> StudentData)
	{
		int ClientID = Multiplayer.GetRemoteSenderId();
		GD.Print($"RPC Received On the Server from {ClientID.ToString()}");
		GD.Print($"Data : {StudentData.Keys} - Received!");
		// CurrentClass.Add(StudentData);
	}

	[RPC(MultiplayerAPI.RPCMode.AnyPeer)]
	public void UpdateClassName(string ClassName)
	{
		AllClassesData.Remove(CurrentClassName);
		AllClassesData.Add(ClassName, CurrentClass);
		CurrentClassName = ClassName;
		GD.Print(CurrentClassName);
	}
	
	[RPC(MultiplayerAPI.RPCMode.AnyPeer)]
	public void RequestClassUnlock(bool isUnlockRequest = true)
	{
		//
	}
	
	[RPC(MultiplayerAPI.RPCMode.AnyPeer)]
	public void OnStudentNameEntered(string NewStudentName)
	{
		AddStudentToCurrentClass(NewStudentName);
	}
	// GD Boilerplate
	
	[RPC]
	public void ProcessClassData(Array<Dictionary<string, string>> ClassData) { }
	
	
	[RPC]
	public void ConfirmServerConnection(string ServerMessage)
	{
		//
	}

	
	[RPC]
	public void CurrentClassUpdate(string StudentName, bool isRemoval = false)
	{
		//
	}
	

	[RPC]
	public void UpdateDataReqest(int RequestType)
	{
		//
	}

	[RPC(MultiplayerAPI.RPCMode.AnyPeer)]
	public void ClassUnlocked(bool isUnlocked = false)
	{
		//
	}

	
}
