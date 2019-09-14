#include <M5StickC.h>
#include <IRremote.h>

IRsend irsend(M5_IR);

bool userDebugEnabled = false;

void setup()
{
  Serial.begin(115200);

  M5.begin();
  lcdBacklightEnable(userDebugEnabled);
  M5.Lcd.println("SerialBlaster");

  pinMode(M5_LED, OUTPUT);
  digitalWrite(M5_LED, LOW);
  digitalWrite(M5_LED, HIGH);

  pinMode(M5_BUTTON_HOME, INPUT);
  pinMode(M5_BUTTON_RST, INPUT);
}

void lcdBacklightEnable(bool enable)
{
  if(enable)
  {
    Wire1.beginTransmission(0x34);
    Wire1.write(0x12);
    Wire1.write(0x4d); // Enable LDO2, aka OLED_VDD
    Wire1.endTransmission();
  }
  else
  {
    Wire1.beginTransmission(0x34);
    Wire1.write(0x12);
    Wire1.write(0b01001011);  // LDO2, aka OLED_VDD, off
    Wire1.endTransmission();
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
      M5.Lcd.printf("%s\n", action);
      if(strcasecmp(action, "send") == 0)
      {
        char *encoding = strtok(NULL, " ");
        if(encoding != NULL)
        {
          M5.Lcd.printf("%s\n", encoding);
          if(strcasecmp(encoding, "nec") == 0)
          {
            char *command = strtok(NULL, " ");
            if(command != NULL)
            {
              M5.Lcd.printf("%s\n", command);
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