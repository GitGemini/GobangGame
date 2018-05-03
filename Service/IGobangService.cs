using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace Service
{
    //需要服务端实现的协定
    [ServiceContract(Namespace = "WcfGobangGameExample",
        SessionMode = SessionMode.Required,
        CallbackContract = typeof(IGobangServiceCallback))]
    public interface IGobangService
    {
        [OperationContract(IsOneWay = true)]
        void Login(string userName);

        [OperationContract(IsOneWay = true)]
        void Logout(string userName);

        [OperationContract(IsOneWay = true)]
        void SitDown(string userName, int index, int side);

        [OperationContract(IsOneWay = true)]
        void GetUp(int index, int side);

        [OperationContract(IsOneWay = true)]
        void Start(string userName, int index, int side);

        [OperationContract(IsOneWay = true)]
        void SetDot(int index, int i, int j);

        //添加内容 二次点击
        [OperationContract(IsOneWay = true)]
        void SetDoubleDot(int index, int table, int i, int j, int nextX, int nextY);

        [OperationContract(IsOneWay = true)]
        void TalkNum(int index, int i, int j);
        //[OperationContract(IsOneWay = true)]
        //void SetGetNext(int i, int j);

        [OperationContract(IsOneWay = true)]
        void Talk(int index, string userName, string message);

        [OperationContract(IsOneWay = true)]
        void Shake(int index, int side); // 震动
    }

    //需要客户端实现的接口
    public interface IGobangServiceCallback
    {

        [OperationContract(IsOneWay = true)]
        void ShowTalkNum(int index, int i, int j);


        [OperationContract(IsOneWay = true)]
        void ShowLogin(string loginUserName, int maxTables);

        [OperationContract(IsOneWay = true)]
        void ShowLogout(string userName);

        [OperationContract(IsOneWay = true)]
        void ShowSitDown(string userName, int side);

        [OperationContract(IsOneWay = true)]
        void ShowGetUp(int side);

        [OperationContract(IsOneWay = true)]
        void ShowStart(int side);

        [OperationContract(IsOneWay = true)]
        void ShowTalk(string userName, string message);

        [OperationContract(IsOneWay = true)]
        void ShowSetDot(int i, int j, int color);

        [OperationContract(IsOneWay = true)]
        void ShowSetDoubleDot(int i, int j, int color, int nextX, int nextY);

        //[OperationContract(IsOneWay = true)]
        //void ShowSetGetNext( int i, int j);

        [OperationContract(IsOneWay = true)]
        void GameStart();

        [OperationContract(IsOneWay = true)]
        void GameWin(string message);

        [OperationContract(IsOneWay = true)]
        void UpdateTablesInfo(string tablesInfo, int userCount);

        [OperationContract(IsOneWay = true)]
        void ShowShake();
    }
}
