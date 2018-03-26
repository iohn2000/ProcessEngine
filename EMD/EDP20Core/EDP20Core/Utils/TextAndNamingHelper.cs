using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Utils
{
    public class TextAndNamingHelper
    {
        [Obsolete("Moved to Kapsch.IS.Util.Strings.StringUtils and renamed to TranslateGermanMutationCharacters(string)")]
        public static String ReplaceGermanUmlaut(String Input_String)
        {
            return Input_String.Replace("ö", "oe").Replace("ä", "ae").Replace("ü", "ue").Replace("Ö", "Oe").Replace("Ä", "Ae").Replace("Ü", "Ue").Replace("ß", "ss");
        }

        [Obsolete("Moved to Kapsch.IS.Util.Strings.StringUtils")]
        public static String RemoveSpecialCharacters(String input)
        {
            //bis 383 in http://www.coreshutdown.com/charsetall.html
            input = input.Trim();
            Int32 iIndex = 0;
            Int32 iLength = input.Length;
            String sChar;
            Int32 iASCII;
            //Int32 iLen;
            //Int32 iRem;
            String Output = "";
            while (iIndex < iLength)
            {
                sChar = input.Substring(iIndex, 1);
                iASCII = Convert.ToInt32(sChar.ToCharArray()[0]);
                if ((iASCII >= 65 && iASCII <= 90) || (iASCII >= 97 && iASCII <= 122)) //|| (iASCII >= 192 && iASCII <= 260) Ausbau wegen tschechischer Umlaute
                {
                    Output = Output + sChar;
                }
                else if (iASCII >= 192 && iASCII <= 198 || iASCII == 193 || iASCII == 196 || iASCII == 256 || iASCII == 258 || iASCII == 260)
                {
                    Output = Output + "A";
                }
                else if (iASCII == 199 || iASCII == 262 || iASCII == 264 || iASCII == 266 || iASCII == 268)
                {
                    Output = Output + "C";
                }
                else if (iASCII == 270 || iASCII == 272)
                {
                    Output = Output + "D";
                }
                else if ((iASCII >= 200 && iASCII <= 203) || iASCII == 201 || iASCII == 268 || iASCII == 274 || iASCII == 276 || iASCII == 278 || iASCII == 280 || iASCII == 282)
                {
                    Output = Output + "E";
                }
                else if (iASCII == 284 || iASCII == 286 || iASCII == 288 || iASCII == 290)
                {
                    Output = Output + "G";
                }
                else if (iASCII == 292 || iASCII == 294)
                {
                    Output = Output + "H";
                }
                else if ((iASCII >= 204 && iASCII <= 207) || iASCII == 205 || iASCII == 296 || iASCII == 298 || iASCII == 300 || iASCII == 302 || iASCII == 304 || iASCII == 306)
                {
                    Output = Output + "I";
                }
                else if (iASCII == 308)
                {
                    Output = Output + "J";
                }
                else if (iASCII == 310)
                {
                    Output = Output + "K";
                }
                else if (iASCII == 313 || iASCII == 315 || iASCII == 317 || iASCII == 319 || iASCII == 321)
                {
                    Output = Output + "L";
                }
                else if (iASCII == 209 || iASCII == 323 || iASCII == 325 || iASCII == 327 || iASCII == 330)
                {
                    Output = Output + "N";
                }
                else if ((iASCII >= 210 && iASCII <= 214) || iASCII == 216 || iASCII == 208 || iASCII == 332 || iASCII == 334 || iASCII == 336 || iASCII == 338)
                {
                    Output = Output + "O";
                }
                else if (iASCII == 340 || iASCII == 342 || iASCII == 344)
                {
                    Output = Output + "R";
                }
                else if (iASCII == 346 || iASCII == 348 || iASCII == 350 || iASCII == 352)
                {
                    Output = Output + "S";
                }
                else if (iASCII == 354 || iASCII == 356 || iASCII == 358)
                {
                    Output = Output + "T";
                }
                else if ((iASCII >= 217 && iASCII <= 220) || iASCII == 360 || iASCII == 362 || iASCII == 364 || iASCII == 366 || iASCII == 368 || iASCII == 370)
                {
                    Output = Output + "U";
                }
                else if (iASCII == 372)
                {
                    Output = Output + "W";
                }
                else if (iASCII == 221 || iASCII == 222 || iASCII == 374 || iASCII == 376)
                {
                    Output = Output + "Y";
                }
                else if (iASCII == 377 || iASCII == 379 || iASCII == 381)
                {
                    Output = Output + "Z";
                }
                else if (iASCII == 223)
                {
                    Output = Output + "ss";
                }
                else if ((iASCII >= 224 && iASCII <= 230) || iASCII == 225 || iASCII == 257 || iASCII == 259 || iASCII == 261 || iASCII == 228)
                {
                    Output = Output + "a";
                }
                else if (iASCII == 231 || iASCII == 263 || iASCII == 265 || iASCII == 267 || iASCII == 269)
                {
                    Output = Output + "c";
                }
                else if (iASCII == 271 || iASCII == 273)
                {
                    Output = Output + "d";
                }
                else if ((iASCII >= 232 && iASCII <= 235) || iASCII == 233 || iASCII == 275 || iASCII == 277 || iASCII == 279 || iASCII == 281 || iASCII == 283)
                {
                    Output = Output + "e";
                }
                else if (iASCII == 293 || iASCII == 287 || iASCII == 289 || iASCII == 291)
                {
                    Output = Output + "g";
                }
                else if (iASCII == 293 || iASCII == 295)
                {
                    Output = Output + "h";
                }
                else if ((iASCII >= 236 && iASCII <= 239) || iASCII == 237 || iASCII == 297 || iASCII == 299 || iASCII == 301 || iASCII == 303 || iASCII == 305 || iASCII == 307)
                {
                    Output = Output + "i";
                }
                else if (iASCII == 309)
                {
                    Output = Output + "j";
                }
                else if (iASCII == 311 || iASCII == 312)
                {
                    Output = Output + "k";
                }
                else if (iASCII == 314 || iASCII == 316 || iASCII == 318 || iASCII == 320 || iASCII == 322)
                {
                    Output = Output + "l";
                }
                else if (iASCII == 241 || iASCII == 324 || iASCII == 326 || iASCII == 328 || iASCII == 329 || iASCII == 331)
                {
                    Output = Output + "n";
                }
                else if ((iASCII >= 242 && iASCII <= 246) || iASCII == 248 || iASCII == 240 || iASCII == 333 || iASCII == 335 || iASCII == 337 || iASCII == 339)
                {
                    Output = Output + "o";
                }
                else if (iASCII == 341 || iASCII == 343 || iASCII == 345)
                {
                    Output = Output + "r";
                }
                else if (iASCII == 353 || iASCII == 347 || iASCII == 349 || iASCII == 351)
                {
                    Output = Output + "s";
                }
                else if (iASCII == 357 || iASCII == 359)
                {
                    Output = Output + "t";
                }
                else if ((iASCII >= 249 && iASCII <= 252) || iASCII == 361 || iASCII == 363 || iASCII == 365 || iASCII == 367 || iASCII == 369 || iASCII == 371)
                {
                    Output = Output + "u";
                }
                else if (iASCII == 373)
                {
                    Output = Output + "w";
                }
                else if ((iASCII >= 253 && iASCII <= 255) || iASCII == 375)
                {
                    Output = Output + "y";
                }
                else if (iASCII == 378 || iASCII == 380 || iASCII == 382)
                {
                    Output = Output + "z";
                }
                iIndex += 1;
            }
            return Output;
        }

        /// <summary>
        /// Checks wether a string contains only characters which are in standard ascii (0-128)
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsC128String(string s)
        {
            return s.All(c => (int)c >= 0 && (int)c <= 128);
        }

    }
}
