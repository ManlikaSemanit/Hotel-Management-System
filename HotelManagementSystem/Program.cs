using HotelManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using static HotelManagementSystem.Models.AppConst;

namespace HotelManagementSystem
{
    public class Program
    {
        public static readonly string HMS_Input_File = ConfigurationManager.AppSettings[HMS_INPUT_FILE_NAME];
        public static readonly string HMS_Output_File = ConfigurationManager.AppSettings[HMS_OUTPUT_FILE_NAME];

        public static List<HotelModel> HotelList = new List<HotelModel>();
        public static List<int> KeyCardList = new List<int>();
        static void Main(string[] args)
        {
            List<string> inputLists = ReadTextFile(HMS_Input_File);
            List<string> outputLists = new List<string>();
            string[] param;
            string strReturnText = "";

            try
            {
                ClareOutputTextFile(HMS_Output_File);

                foreach (string input in inputLists)
                {
                    param = input.Split(' ');
                    switch (param[0])
                    {
                        case string s when s.Equals(CREATE_HOTEL):
                            strReturnText = CreateHotel(Convert.ToInt32(param[1]), Convert.ToInt32(param[2]));
                            outputLists.Add(strReturnText);
                            break;
                        case string s when s.Equals(BOOK):
                            strReturnText = Book(false, param[1], param[2], Convert.ToInt32(param[3]));
                            outputLists.Add(strReturnText);
                            break;
                        case string s when s.Equals(BOOK_BY_FLOOR):
                            strReturnText = Book(true, "", param[2], Convert.ToInt32(param[3]), Convert.ToInt32(param[1]));
                            outputLists.Add(strReturnText);
                            break;
                        case string s when s.Equals(LIST_AVAILABLE_ROOMS):
                            strReturnText = GetHotelData("AVAILABLE_ROOMS");
                            outputLists.Add(strReturnText);
                            break;
                        case string s when s.Equals(LIST_GUEST):
                            strReturnText = GetHotelData("GUEST");
                            outputLists.Add(strReturnText);
                            break;
                        case string s when s.Equals(GET_GUEST_IN_ROOM):
                            strReturnText = GetHotelData("GUEST_IN_ROOM", param[1]);
                            outputLists.Add(strReturnText);
                            break;
                        case string s when s.Equals(LIST_GUEST_BY_AGE):
                            strReturnText = GetHotelData("GUEST_BY_AGE", "", param[1], Convert.ToInt32(param[2]));
                            outputLists.Add(strReturnText);
                            break;
                        case string s when s.Equals(LIST_GUEST_BY_FLOOR):
                            strReturnText = GetHotelData("GUEST_BY_FLOOR", "", "", 0, Convert.ToInt32(param[1]));
                            outputLists.Add(strReturnText);
                            break;
                        case string s when s.Equals(CHECKOUT):
                            strReturnText = Checkout(false, 0, Convert.ToInt32(param[1]), param[2]);
                            outputLists.Add(strReturnText);
                            break;
                        case string s when s.Equals(CHECKOUT_GUEST_BY_FLOOR):
                            strReturnText = Checkout(true, Convert.ToInt32(param[1]));
                            outputLists.Add(strReturnText);
                            break;
                        default:
                            return;
                    }
                }

                WriteTextFile(HMS_Output_File, outputLists);
            }
            catch (Exception ex)
            {
                outputLists.Add("Error : " + ex.Message);
                WriteTextFile(HMS_Output_File, outputLists);
            }
        }

        #region Read/Write/Clear Text File

        private static List<string> ReadTextFile(string strFileName)
        {
            string strPath = Environment.CurrentDirectory.Split(new String[] { "bin" }, StringSplitOptions.None)[0] + strFileName;
            List<string> textList = new List<string>();
            using (StreamReader _readFile = new StreamReader(strPath))
            {
                string ln;

                while (null != (ln = _readFile.ReadLine()))
                {
                    if(!string.IsNullOrEmpty(ln))
                        textList.Add(ln);
                }
                _readFile.Close();
            }

            return textList;
        }

        private static void WriteTextFile(string strFileName, List<string> strWriteTextList)
        {
            string strPath = Environment.CurrentDirectory.Split(new String[] { "bin" }, StringSplitOptions.None)[0] + strFileName;
            using (StreamWriter _writeFile = new StreamWriter(strPath))
            {
                foreach (string text in strWriteTextList)
                {
                    if (!string.IsNullOrEmpty(text))
                        _writeFile.WriteLine(text);
                }
                _writeFile.Close();
            }
        }

        private static void ClareOutputTextFile(string strFileName)
        {
            string strPath = Environment.CurrentDirectory.Split(new String[] { "bin" }, StringSplitOptions.None)[0] + strFileName;

            File.Create(strPath).Close();
        }

        #endregion Read/Write/Clear Text File

        #region Activity

        private static string CreateHotel(int? intFloors, int? intRooms)
        {
            string strText = "";
            int intKeyCardNo = 1;
            try
            {
                for (int floor = 1; floor <= intFloors; floor++)
                {
                    for (int room = 1; room <= intRooms; room++)
                    {
                        HotelModel hotelModel = new HotelModel();
                        hotelModel.Floor = floor;
                        hotelModel.Room = floor.ToString() + room.ToString().PadLeft(2, '0');
                        hotelModel.isBooked = false;
                        hotelModel.KeyCardNo = null;
                        hotelModel.BookedBy = null;
                        hotelModel.BookedByAge = null;

                        HotelList.Add(hotelModel);
                        KeyCardList.Add(intKeyCardNo);

                        intKeyCardNo++;
                    }
                }

                strText = String.Format("Hotel created with {0} floor(s), {1} room(s) per floor.", intFloors, intRooms);
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
            return strText;
        }

        private static string Book(bool? isBookByFloor, string strRoom, string strBookBy, int? strBookByAge, int? intFloor = 0)
        {
            string strText = "";
            int? intKeyCardNo = KeyCardList.Min();
            string strRoomList = "";
            string strKeyCardList = "";

            try
            {
                if (isBookByFloor == false)
                {
                    HotelModel hotelModel = HotelList.Where(w => w.Room == strRoom).FirstOrDefault();
                    if(hotelModel != null)
                    {
                        if (hotelModel.isBooked == true)// Already booked by other customer
                        {
                            strText = String.Format("Cannot book room {0} for {1}, The room is currently booked by {2}.", hotelModel.Room, strBookBy, hotelModel.BookedBy);
                        }
                        else
                        {
                            hotelModel.isBooked = true;
                            hotelModel.KeyCardNo = intKeyCardNo;
                            KeyCardList.Remove((int)intKeyCardNo);
                            hotelModel.BookedBy = strBookBy;
                            hotelModel.BookedByAge = strBookByAge;

                            strText = String.Format("Room {0} is booked by {1} with keycard number {2}.", hotelModel.Room, hotelModel.BookedBy, hotelModel.KeyCardNo);
                        }
                    }
                }
                else
                {
                    if (HotelList.Where(w => w.Floor == intFloor && w.isBooked == true).Any())// Already booked by other customer
                    {
                        strText = String.Format("Cannot book floor {0} for {1}.", intFloor, strBookBy, strBookBy);
                    }
                    else
                    {
                        foreach (HotelModel hotelModel in HotelList.Where(w => w.Floor == intFloor).ToList())
                        {
                            intKeyCardNo = KeyCardList.Min();
                            hotelModel.isBooked = true;
                            hotelModel.KeyCardNo = intKeyCardNo;
                            KeyCardList.Remove((int)intKeyCardNo);
                            hotelModel.BookedBy = strBookBy;
                            hotelModel.BookedByAge = strBookByAge;

                            strRoomList = string.IsNullOrEmpty(strRoomList) ? hotelModel.Room : strRoomList + ", " + hotelModel.Room;
                            strKeyCardList = string.IsNullOrEmpty(strKeyCardList) ? hotelModel.KeyCardNo.ToString() : strKeyCardList + ", " + hotelModel.KeyCardNo.ToString();

                        }
                        strText = String.Format("Room {0} are booked with keycard number {1}", strRoomList, strKeyCardList);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            

            return strText;
        }

        private static string GetHotelData(string strGetCon, string strRoom = "", string strCon = "", int? intAge = 0, int? intFloor = 0)
        {
            string strText = ""; 
            try
            {
                switch (strGetCon)
                {
                    case "AVAILABLE_ROOMS":
                        strText = string.Join(", ", HotelList.Where(w => w.isBooked == false).Select(s => s.Room));
                        break;
                    case "GUEST":
                        strText = string.Join(", ", HotelList.Where(w => w.isBooked == true).OrderBy(o => o.KeyCardNo).Select(s => s.BookedBy).Distinct());
                        break;
                    case "GUEST_IN_ROOM":
                        strText = string.Join(", ", HotelList.Where(w => w.isBooked == true && w.Room == strRoom).Select(s => s.BookedBy).Distinct());
                        break;
                    case "GUEST_BY_AGE":
                        if(strCon == "<")
                            strText = string.Join(", ", HotelList.Where(w => w.isBooked == true && w.BookedByAge < intAge).Select(s => s.BookedBy).Distinct());
                        else if(strCon == ">")
                            strText = string.Join(",  ", HotelList.Where(w => w.isBooked == true && w.BookedByAge > intAge).Select(s => s.BookedBy).Distinct());
                        else strText = string.Join(",  ", HotelList.Where(w => w.isBooked == true && w.BookedByAge == intAge).Select(s => s.BookedBy).Distinct());
                        break;
                    case "GUEST_BY_FLOOR":
                        strText = string.Join(",  ", HotelList.Where(w => w.isBooked == true && w.Floor == intFloor).Select(s => s.BookedBy).Distinct());
                        break;
                    default:
                        return strText;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
            return strText;
        }

        private static string Checkout(bool? isCheckoutByFloor, int? intFloor = 0, int? intKeyCardNo = 0, string strBookBy = "")
        {
            string strText = "";
            string strRoomList = "";

            try
            {
                if (isCheckoutByFloor == false)
                {
                    HotelModel hotelModel = HotelList.Where(w => w.KeyCardNo == intKeyCardNo).FirstOrDefault();
                    if (hotelModel != null)
                    {
                        if (hotelModel.BookedBy == strBookBy)
                        {
                            hotelModel.isBooked = false;
                            KeyCardList.Add((int)hotelModel.KeyCardNo);
                            hotelModel.KeyCardNo = null;
                            hotelModel.BookedBy = null;
                            hotelModel.BookedByAge = null;

                            strText = String.Format("Room {0} is checkout.", hotelModel.Room);
                        }
                        else
                        {
                            strText = String.Format("Only {0} can checkout with keycard number {1}.", hotelModel.BookedBy, hotelModel.KeyCardNo);
                        }
                    }
                }
                else
                {
                    foreach (HotelModel hotelModel in HotelList.Where(w => w.Floor == intFloor && w.isBooked == true).ToList())
                    {
                        hotelModel.isBooked = false;
                        KeyCardList.Add((int)hotelModel.KeyCardNo);
                        hotelModel.KeyCardNo = null;
                        hotelModel.BookedBy = null;
                        hotelModel.BookedByAge = null;

                        strRoomList = string.IsNullOrEmpty(strRoomList) ? hotelModel.Room: strRoomList + ", " + hotelModel.Room;
                    }

                    strText = String.Format("Room {0} are checkout.", strRoomList);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
            return strText;
        }

        #endregion Activity
    }
}
