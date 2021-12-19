using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for ClerkNameSingelton
/// </summary>
public class ClerkNameSingelton
{
    private static ClerkNameSingelton instance = null;
    private static string mId = string.Empty;

    public static string getId()
    {
        return mId;
    }

	private ClerkNameSingelton()
	{
		
	}

    public static ClerkNameSingelton getClerkI()
    {
        if (instance == null)
        {
             instance = new ClerkNameSingelton();
        }

        return instance;
    }

    public static void setName(string iId)
    {
        mId = iId;
    }
}