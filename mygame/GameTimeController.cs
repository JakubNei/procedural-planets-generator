using System;

namespace MyGame
{
    public class GameTimeController
    {
        public static GameTimeController Instance { get; private set; }

        public float CurrentHour
        {
            get
            {
                return currentDateTime.Hour + currentDateTime.Minute / 60f + currentDateTime.Second / 60f / 60f;
            }
            set
            {
                currentDateTime = DateTimeSetHour(currentDateTime, value, moveOnlyForwardInTime: true);
            }
        }        

        /// <summary>
        /// If set to 1 it takes 1 second in real life to pass 1 ingame second.
        /// If its set to 10 it takes 1 second in real life to pass 10 ingame second. 
        /// </summary>
        public float CurrentGameTimeSpeed { get; set; }


        DateTime currentDateTime;

        DateTime dateTimeToSkipTo;
        bool doSkipTime;
        float skipSpeed;


        public void SkipHourTo(int hourToSkipTo, float skipSpeed = 100f)
        {
            doSkipTime = true;
            this.skipSpeed = skipSpeed;
            dateTimeToSkipTo = DateTimeSetHour(currentDateTime, hourToSkipTo, moveOnlyForwardInTime: true);
        }


        static DateTime DateTimeSetHour(DateTime dateTime, float targetHour, bool moveOnlyForwardInTime = false)
        {
            if (dateTime.Hour > targetHour)
            {
                // hour is alreardy past the one we need, either end day, or rever time
                if (moveOnlyForwardInTime)
                {
                    // current hour is too far, must end the day
                    dateTime.AddHours(24 - dateTime.Hour); // end the day, go to 0:00;
                    dateTime.AddHours(targetHour); // go to our target hour
                } else
                {
                    // lets revert time
                    dateTime.AddHours(targetHour - dateTime.Hour);
                }
            }
            else
            {
                // target hour is in the future, add it
                dateTime.AddHours(dateTime.Hour - targetHour);
            }
            return dateTime;
        }



        void Start()
        {
            CurrentGameTimeSpeed = 1;
            currentDateTime = DateTime.Now;
            Instance = this;
        }

        void Update()
        {
            if (doSkipTime)
            {
                CurrentGameTimeSpeed = skipSpeed;
                if (currentDateTime > dateTimeToSkipTo)
                {
                    doSkipTime = false;
                    CurrentGameTimeSpeed = 1;
                }
            }

            //currentDateTime.AddSeconds(Time.deltaTime * CurrentGameTimeSpeed);
            //weatherSystem.azure_PassTime = CurrentGameTimeSpeed;
            //weatherSystem.azure_TIME_of_DAY = CurrentHour;
        }

        //public SkyControl_v2_0_AnimatedCloud weatherSystem;
    }
}
