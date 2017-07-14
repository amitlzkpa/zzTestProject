using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

public class Foo : MonoBehaviour
{



    //------------------------------------------------------


    // The latitude conversion is still buggy

    //private string DMS_topEdgeCoord = "";
    //private string DMS_leftEdgeCoord = "";
    //private string DMS_bottomEdgeCoord = "";
    //private string DMS_rightEdgeCoord = "";


    //// ref: https://en.wikipedia.org/wiki/Geographic_coordinate_conversion
    //private string asDMSCoords(float DD_coord, bool isLat)
    //{
    //    int deg = Mathf.FloorToInt(DD_coord);
    //    int min = Mathf.FloorToInt((60 * (DD_coord - deg)));
    //    float sec = 3600 * (DD_coord - deg) - (60 * min);
    //    string dir = (isLat) ? (DD_coord > 0) ? "N" : "S" : (DD_coord > 0) ? "E" : "W";
    //    return string.Format("{0}° {1}' {2:0.0}\" {3}", deg, min, sec, dir);
    //}


    //------------------------------------------------------








    private string downloadFolderPath;
    private string downloadFileName = "myterrain";
    private string downloadFileExtension = "zip";
    private string downloadFilePath;
    
    public Vector2 DD_center = new Vector2(40.706968f, -74.009675f);
    public float squareSide = 2000;



    // ref: https://gis.stackexchange.com/questions/5821/calculating-latitude-longitude-x-miles-from-point
    // 
    private Vector2 getOffsetCoords(Vector2 inLoc, float XoffsetInMetres, float YoffsetInMetres)
    {
        Vector2 retVec = Vector2.zero;
        float convFacInMetres = 111111f;
        float bearingN = Mathf.Cos(0); // for northwards offset
        float bearingE = Mathf.Sin(Mathf.PI / 2); // for sideways offset
        retVec.x = inLoc.x + (XoffsetInMetres * (bearingN / convFacInMetres));
        retVec.y = inLoc.y + (YoffsetInMetres * ((bearingE / Mathf.Cos(Mathf.Deg2Rad * (inLoc.x))) / convFacInMetres));
        return retVec;
    }


    private void getHeightMap()
    {
        Vector2 tL = getOffsetCoords(DD_center, (squareSide / 2), -(squareSide / 2));
        Vector2 bR = getOffsetCoords(DD_center, -(squareSide / 2), (squareSide / 2));

        string fullQueryString = string.Format("http://terrain.party/api/export?name={0}&box={1},{2},{3},{4}", downloadFileName,
                                                                                                               tL.y.ToString("F6"),
                                                                                                               tL.x.ToString("F6"),
                                                                                                               bR.y.ToString("F6"),
                                                                                                               bR.x.ToString("F6"));
        Debug.Log("Querying..." + fullQueryString);
        WWW www = new WWW(fullQueryString);
        StartCoroutine(WaitForRequest(www));
    }




    IEnumerator WaitForRequest(WWW www)
    {
        yield return www;

        // check for errors
        if (www.error == null)
        {
            byte[] dwnBytes = www.bytes;
            File.WriteAllBytes(downloadFilePath, dwnBytes);

            FileInfo fileToDecompress = new FileInfo(downloadFilePath);

            DirectoryInfo decompressFolder = new DirectoryInfo(downloadFolderPath + downloadFileName);
            decompressFolder.Create();

            FastZip z = new FastZip();
            z.ExtractZip(fileToDecompress.FullName, decompressFolder.FullName, null);


            FileInfo reqF = null;
            foreach(FileInfo f in decompressFolder.GetFiles())
            {
                if (f.Name.ToLower().Contains("merge"))
                {
                    reqF = f;
                    Debug.Log(reqF);
                    break;
                }
            }

            fileToDecompress.Delete();
            foreach(FileInfo fl in decompressFolder.GetFiles())
            {
                if (!(fl.FullName == reqF.FullName))
                {
                    fl.Delete();
                }
            }
            Debug.Log("Done");
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
        }
    }



    // Use this for initialization
    void Start () {
        downloadFolderPath = Directory.GetCurrentDirectory() + "/Assets/project02/downloads/";
        downloadFilePath = downloadFolderPath + downloadFileName + "." + downloadFileExtension;
    }
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Space))
        {
            getHeightMap();
        }
	}
}
