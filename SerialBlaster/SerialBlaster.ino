#include <M5StickC.h>
#include <IRremote.h>

IRsend irsend(M5_IR);

void setup()
{
  Serial.begin(115200);
  Serial.setTimeout(60 * 1000);

  M5.begin();
  M5.Lcd.println("SerialBlaster");

  pinMode(M5_LED, OUTPUT);
  digitalWrite(M5_LED, LOW);
  digitalWrite(M5_LED, HIGH);

  delay(4000);
}

void loop()
{
  String in;
  while(Serial.available())
  {
    in = Serial.readStringUntil('\r');

    delay(250);
    
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

              digitalWrite(M5_LED, LOW);
              delay(50);
              digitalWrite(M5_LED, HIGH);
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