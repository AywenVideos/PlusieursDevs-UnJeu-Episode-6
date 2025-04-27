namespace Habbo_Common
{
    public enum MessageTypes
    {
        ToServer_Register,
        ToServer_Login,

        ToServer_GetPlayer,
        ToClient_GetPlayers,

        ToServer_Teleport,
        ToClient_Teleport,

        ToServer_PlayerMove,
        ToClient_PlayerMove,

        ToServer_PlayerSetSkin,
        ToClient_PlayerSetSkin,
        ToServer_PlayerSetColor,
        ToClient_PlayerSetColor,

        ToServer_PoseBlock,
        ToServer_BreakBlock,
        ToClient_SendBlocks,
        ToServer_GetBlocks,

        ToServer_Chat,
        ToClient_Chat,

        ToServer_Rename,
        ToClient_Rename,

        ToServer_AddGold,
        ToClient_AddGold,
    }
}