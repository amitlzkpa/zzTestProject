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
    private string terrDownloadFileName = "myterrain";
    private string terrDownloadFileExtension = "zip";
    private string terrDownloadFilePath;

    private string sattDownloadFileName = "satt";
    private string sattDownloadFileExtension = "png";
    private string sattDownloadFilePath;
    
    public Vector2 DD_center;
    public float squareSide;



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

        string terrainQueryString = string.Format("http://terrain.party/api/export?name={0}&box={1},{2},{3},{4}", terrDownloadFileName,
                                                                                                               tL.y.ToString("F6"),
                                                                                                               tL.x.ToString("F6"),
                                                                                                               bR.y.ToString("F6"),
                                                                                                               bR.x.ToString("F6"));
        string satteliteImageQueryString = string.Format("https://maps.googleapis.com/maps/api/staticmap?center={0},{1}&zoom=13&format=png32&size=1081x1081&maptype=satellite&key={2}", DD_center.x.ToString("F6"), DD_center.y.ToString("F6"), "AIzaSyB6BE6Ryi4Qc83XgN1g-d0ePrvWoINNsnU");

        WWW wwwTerrain = new WWW(terrainQueryString);
        StartCoroutine(WaitForTerrainImage(wwwTerrain));
        WWW wwwSatellite = new WWW(satteliteImageQueryString);
        StartCoroutine(WaitForSatelliteImage(wwwSatellite));
    }




    IEnumerator WaitForSatelliteImage(WWW www)
    {
        Debug.Log("Querying for satellite image..." + www.url);
        yield return www;

        // check for errors
        if (www.error == null)
        {
            byte[] dwnBytes = www.bytes;
            File.WriteAllBytes(sattDownloadFilePath, dwnBytes);
            Debug.Log("Done terrain.");
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
        }
    }




    IEnumerator WaitForTerrainImage(WWW www)
    {
        Debug.Log("Querying for terrain image..." + www.url);
        yield return www;

        // check for errors
        if (www.error == null)
        {
            byte[] dwnBytes = www.bytes;
            File.WriteAllBytes(terrDownloadFilePath, dwnBytes);

            FileInfo fileToDecompress = new FileInfo(terrDownloadFilePath);

            DirectoryInfo decompressFolder = new DirectoryInfo(downloadFolderPath + terrDownloadFileName);
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
            Debug.Log("Done terrain.");
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
        }
    }



    // Use this for initialization
    void Start () {
        downloadFolderPath = Directory.GetCurrentDirectory() + "/Assets/project02/downloads/";
        terrDownloadFilePath = downloadFolderPath + terrDownloadFileName + "." + terrDownloadFileExtension;
        sattDownloadFilePath = downloadFolderPath + sattDownloadFileName + "." + sattDownloadFileExtension;
    }
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Space))
        {
            getHeightMap();
        }
	}
}
