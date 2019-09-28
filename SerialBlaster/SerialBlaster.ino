#include <M5StickC.h>
#include <IRremote.h>

IRsend irsend(M5_IR);

bool userDebugEnabled = false;
unsigned int cursorYMax = 160;

void setup()
{
  Serial.begin(115200);

  M5.begin();
  lcdBacklightEnable(userDebugEnabled);
  M5.Lcd.setTextWrap(true);
  lcdPrintln("SerialBlaster");

  pinMode(M5_LED, OUTPUT);
  digitalWrite(M5_LED, LOW);
  digitalWrite(M5_LED, HIGH);

  pinMode(M5_BUTTON_HOME, INPUT);
  pinMode(M5_BUTTON_RST, INPUT);
}

size_t lcdPrintln(const char *string)
{
    lcdManageVerticalWrap();

    size_t len = M5.Lcd.print(string);
    len += M5.Lcd.println();
    return len;
}

size_t lcdPrintf(const char *format, ...)
{
    lcdManageVerticalWrap();

    char loc_buf[64];
    char * temp = loc_buf;
    va_list arg;
    va_list copy;
    va_start(arg, format);
    va_copy(copy, arg);
    size_t len = vsnprintf(NULL, 0, format, arg);
    va_end(copy);
    if(len >= sizeof(loc_buf)){
        temp = new char[len+1];
        if(temp == NULL) {
            return 0;
        }
    }
    len = vsnprintf(temp, len+1, format, arg);
    M5.Lcd.print(temp);
    va_end(arg);
    if(len >= sizeof(loc_buf)){
        delete[] temp;
    }
    return len;
}

void lcdManageVerticalWrap()
{
  int16_t cursorY = M5.Lcd.getCursorY();
  if(cursorY >= cursorYMax)
  {
    M5.Lcd.fillScreen(0x000000);
    M5.Lcd.setCursor(0, 0);
  }
}

void lcdBacklightEnable(bool enable)
{
  if(enable)
  {
    M5.Axp.ScreenBreath(15); 
  }
  else
  {
    M5.Axp.ScreenBreath(0) ;
  }
}

void loop()
{
  String in;

  static int buttonHomePrevious = HIGH;
  int buttonHome = digitalRead(M5_BUTTON_HOME);

  static int buttonResetPrevious = HIGH;
  int buttonReset = digitalRead(M5_BUTTON_RST);

  if(buttonHomePrevious == HIGH && buttonHome == LOW)
  {
      userDebugEnabled = !userDebugEnabled;
      lcdBacklightEnable(userDebugEnabled);
  }
  buttonHomePrevious = buttonHome;

  if(buttonResetPrevious == HIGH && buttonReset == LOW)
  {
    ESP.restart();
  }
  buttonResetPrevious = buttonReset;
  
  while(Serial.available())
  {
    in = Serial.readStringUntil('\r');

    //Example
    //send nec 0x3EC14DB2

    char *action = strtok((char*)in.c_str(), " ");
    if (action != NULL)
    {
      lcdPrintf("%s\n", action);
      if(strcasecmp(action, "send") == 0)
      {
        char *encoding = strtok(NULL, " ");
        if(encoding != NULL)
        {
          lcdPrintf("%s\n", encoding);
          if(strcasecmp(encoding, "nec") == 0)
          {
            char *command = strtok(NULL, " ");
            if(command != NULL)
            {
              lcdPrintf("%s\n", command);
              unsigned long commandUL;
              commandUL = strtoul(command, NULL, 16);
              irsend.sendNEC(commandUL, 32);
              Serial.println("OK");

              if(userDebugEnabled)
              {
                digitalWrite(M5_LED, LOW);
                delay(50);
                digitalWrite(M5_LED, HIGH);
              }
            }
            else
            {
              Serial.println("ERROR: No command specified.");
            }
          }
          else
          {
            Serial.printf("ERROR: Unknown encoding '%s'.\r\n", encoding);
          }
        }
        else
        {
          Serial.println("ERROR: No encoding specified.");
        }
      }
      else if(strcasecmp(action, "message") == 0)
      {
        char *message = strtok(NULL, " ");
        if(message != NULL)
        {
          lcdPrintf("%s\n", message);
          Serial.println("OK");
        }
        else
        {
          Serial.println("ERROR: No message specified.");
        }
      }
      else
      {
        Serial.printf("ERROR: Unknown action '%s'.\r\n", action);
      }
    }
    else
    {
      Serial.println("ERROR: Empty message.");
    }

    Serial.flush();
  }
}