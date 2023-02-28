// the setup function runs once when you press reset or power the board
int LED = 3;
void setup() {
  // initialize digital pin LED_BUILTIN as an output.
  Serial.begin(9600);
  pinMode(LED, OUTPUT);
  analogWrite(LED, 255);
}


// the loop function runs over and over again forever
void loop() {
  if (Serial.available())
  {
    Serial.print("received");
    String valueStr = Serial.readString();
    int value = valueStr.toInt();
    analogWrite(LED, value);  // turn the LED on (HIGH is the voltage level)
    Serial.print(value);
    delay(100);
  }
}
