#include <M5StickC.h>
#include <IRremote.h>

//Examples
//s0x3EC14DB2

IRsend irsend(M5_IR);

void setup()
{
  Serial.begin(115200);
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
    in = Serial.readString(); // read the incoming data as string
    const char* pos = in.c_str();
    M5.Lcd.println(in.c_str());
    
    if(*pos == 's') //send
    {
      pos++;
      unsigned long command;
      strtoul(pos, NULL, 16);
      irsend.sendNEC(command, 32);
      Serial.println("OK");
     
      digitalWrite(M5_LED, LOW);
      delay(1000);
      digitalWrite(M5_LED, HIGH);
    }
    else
    {
      Serial.println("ERROR: Unknown message type.");
    }
  }
}