# include <Adafruit_MotorShield.h> //<callout id="code.curtainautomation.includes"/>
# include <Wire.h>
# include "utility/Adafruit_PWMServoDriver.h"
# include <DHT11.h>
# include <LiquidCrystal.h>
# include <IRremote.h>
# include <IRremoteInt.h>
//START:defines
#define LIGHT_PIN         0  //<callout id="code.curtainautomation.defines"/>
#define LIGHT_THRESHOLD 600
#define TEMP_PIN          5
#define TEMP_THRESHOLD   26
#define TEMP_VOLTAGE    5.0
#define ONBOARD_LED      13
#define irPin             6
DHT11 dht11(TEMP_PIN);
LiquidCrystal lcd(12,11,10,9,8,7);
//END:defines

//START:variables
int curtain_state = 0; // <callout id="code.curtainautomation.variables"/>
int light_status = 0;
double temp_status = 0;

boolean daylight = true;
boolean warm = false;

IRrecv irrecv(irPin);
decode_results results;

Adafruit_MotorShield AFMS = Adafruit_MotorShield();
Adafruit_StepperMotor* motor = AFMS.getStepper(200, 2);
//END: variables

//START:setup
void setup()
{ //<callout id="code.curtainautomation.setup"/>
    lcd.begin(16, 2);
    irrecv.enableIRIn();
    Serial.begin(9600);
    AFMS.begin();
    Serial.println("Setting up Curtain Automation...");
    // Set stepper motor rotation speed to 100 RPMs
    motor->setSpeed(10);
    // Initialize motor
    // motor.step(100, FORWARD, SINGLE); 
    // motor.release();
    delay(500);
}
//END:setup

//START:curtain
void Curtain(boolean curtain_state)
{   //<callout id="code.curtainautomation.curtain"/>
    digitalWrite(ONBOARD_LED, curtain_state ? HIGH : LOW);
    if (curtain_state)
    {
        Serial.println("Opening curtain...");
        // Try SINGLE, DOUBLE, INTERLEAVE or MICROSTOP
        motor->step(100, FORWARD, DOUBLE);
    }
    else
    {
        Serial.println("Closing curtain...");
        motor->step(100, BACKWARD, DOUBLE);
    }
}
//END:curtain

//START:main_loop
void loop()
{  //<callout id="code.curtainautomation.mainloop"/>

    // poll photocell value
    light_status = analogRead(LIGHT_PIN);
    delay(1000);

    // print light_status value to the serial port
    Serial.print("Photocell value = ");
    Serial.println(light_status);
    Serial.println("");
    lcd.setCursor(0, 0);
    lcd.print("Nasl:");
    lcd.setCursor(5, 0);
    lcd.print(light_status);

    // poll temperature
    int err;
    float temp, humi;
    float temp_Fahrenheit = (temp * 9 / 5) + 32;
    if ((err = dht11.read(humi, temp)) == 0)
    {
        Serial.print("temperature:");
        Serial.print(temp);
        Serial.print(" humidity:");
        Serial.print(humi);
        Serial.println();
        Serial.print("Temperature value (Fahrenheit) = ");
        Serial.println(temp_Fahrenheit);
        Serial.println("");
        lcd.setCursor(0, 1);
        lcd.print("Wilg:");
        lcd.setCursor(5, 1);
        lcd.print(humi);
        lcd.setCursor(9, 0);
        lcd.print("Temp:");
        lcd.setCursor(14, 0);
        lcd.print(temp);

        //for (int positionCounter = 0; positionCounter < 16; positionCounter++) {
        // scroll one position left:
        //lcd.scrollDisplayLeft();
        // wait a bit:
        //delay(50);
        //  }

    }

    else
    {
        Serial.println();
        Serial.print("Error No :");
        Serial.print(err);
        Serial.println();
    }
    delay(DHT11_RETRY_DELAY); //delay for reread
                              /*
                              int temp_reading = analogRead(TEMP_PIN);
                              delay(1000);

                              // convert voltage to temp in Celsius and Fahrenheit
                              float voltage = (temp_reading * TEMP_VOLTAGE) / 1024.0;  
                              float temp_Celsius = (voltage - 0.5) * 100 ;
                              float temp_Fahrenheit = (temp_Celsius * 9 / 5) + 32;
                              // print temp_status value to the serial port
                              Serial.print("Temperature value (Celsius) = ");
                              Serial.println(temp_Celsius);
                              Serial.print("Temperature value (Fahrenheit) = ");
                              Serial.println(temp_Fahrenheit);
                              Serial.println("");
                              */
                              // if(!button) { action = true; }
    if (irrecv.decode(&results))
    {
        switch (results.value)
        {
            /*case 0x1FE48B7:
               Serial.println("ESC");
               break;

            case 0x1FE50AF:
               Serial.println("1");
               break;

            case 0x1FED827:
               Serial.println("2");
               break;

            case 0x1FEF807:
               Serial.println("3");
               break;

            case 0x1FE30CF:
               Serial.println("4");
               break;

             case 0x1FEB04F:
               Serial.println("5");
               break;  

             case 0x1FE708F:
               Serial.println("6");
               break;

             case 0x1FE00FF:
               Serial.println("7");
               break;

             case 0x1FEF00F:
               Serial.println("8");
               break;

             case 0x1FE9867:
               Serial.println("9");
               break;

             case 0x1FEE01F:
               Serial.println("0");
               break;

             case 0x1FEA05F:
               Serial.println("+");
               break;

             case 0x1FE20DF:
               Serial.println("-");
               break;*/

            case 0x1FE807F:
                motor->step(300, BACKWARD, SINGLE);
                //Serial.println("PREV");
                delay(300);
                break;

            case 0x1FE40BF:
                motor->step(300, FORWARD, SINGLE);
                //Serial.println("NEXT");
                delay(300);
                break;
        }

        irrecv.resume();
    }


    if (light_status > LIGHT_THRESHOLD)
        daylight = true;
    else
        daylight = false;
    if (temp > TEMP_THRESHOLD)
        //if (temp_Fahrenheit > TEMP_THRESHOLD)
        warm = true;
    else
        warm = false;

    switch (curtain_state)
    {
        case 0:
            if (daylight && !warm)
            // open curtain
            {
                curtain_state = 1;
                Curtain(curtain_state);
                // lcd.print("Open Curtain");
                // delay(2500);
                // lcd.clear();
            }
            break;

        case 1:
            if (!daylight || warm)
            // close curtain
            {
                curtain_state = 0;
                Curtain(curtain_state);
                // lcd.print("Close Curtain");
                // delay(2500);
                // lcd.clear();
            }
            break;
    }

}
//END:main_loop
