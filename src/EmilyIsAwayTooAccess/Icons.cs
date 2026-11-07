using System.Collections.Generic;

namespace EmilyIsAwayTooAccess
{
    // Names and short picture descriptions for every buddy icon (they're small pixel-art
    // pictures with no text in the game). Written by looking at each picture.
    internal static class Icons
    {
        private static readonly Dictionary<BuddyIconType, string[]> All = new Dictionary<BuddyIconType, string[]>
        {
            { BuddyIconType.Default, new[] { "Default", "a grey silhouette of a person on orange" } },

            // Chapter 1, August 2006
            { BuddyIconType.C1SnowPatrol, new[] { "Snow Patrol", "a light blue snowflake on black" } },
            { BuddyIconType.C1SensesFail, new[] { "Senses Fail", "two men shaking hands, one of them on fire" } },
            { BuddyIconType.C1Ratatat, new[] { "Ratatat", "a cream line drawing of a lion's head on black" } },
            { BuddyIconType.C1Summer, new[] { "Summer", "a beach and a blue sky over the word summer" } },
            { BuddyIconType.C1Mogwai, new[] { "Mogwai", "a dark, blurry crowd of figures" } },
            { BuddyIconType.C1HighSchool, new[] { "High School Musical", "the words High School Musical in big red letters" } },
            { BuddyIconType.C1TakingBackSunday, new[] { "Taking Back Sunday", "the letters T.B.S. over a brown figure" } },
            { BuddyIconType.C1SeanPaul, new[] { "Sean Paul", "the name Sean Paul in white over a red heart" } },
            { BuddyIconType.C1TheFray, new[] { "The Fray", "the word Fray over a glowing golden tree" } },
            { BuddyIconType.C1Nickleback, new[] { "Nickelback", "an empty highway running into a grey sky" } },
            { BuddyIconType.C1Sunshine, new[] { "Little Miss Sunshine", "a yellow camper van over the word Sunshine" } },
            { BuddyIconType.C1Vendetta, new[] { "V for Vendetta", "the grinning Guy Fawkes mask, drawn in black on red" } },
            { BuddyIconType.C170sShow, new[] { "That '70s Show", "the colourful That '70s Show logo" } },
            { BuddyIconType.C1Sopranos, new[] { "The Sopranos", "the word Sopranos in black on red" } },
            { BuddyIconType.C1Sims2, new[] { "The Sims 2", "The Sims 2 logo with its green diamond" } },
            { BuddyIconType.C1Oblivion, new[] { "Oblivion", "the dark Oblivion emblem on old paper" } },
            { BuddyIconType.C1Leekspin, new[] { "Leekspin", "a laughing anime girl with orange hair spinning a leek" } },
            { BuddyIconType.C1HeMan, new[] { "He-Man", "a grinning, blond He-Man in front of a rainbow" } },

            // Chapter 2, October 2006
            { BuddyIconType.C2SensesFail, new[] { "Senses Fail", "a dark face with a red bar across the eyes" } },
            { BuddyIconType.C2SigurRos, new[] { "Sigur Rós", "scratchy black symbols on cream" } },
            { BuddyIconType.C2Mastodon, new[] { "Mastodon", "a hooded figure with red eyes and a green grin" } },
            { BuddyIconType.C2HappyFeet, new[] { "Happy Feet", "a smiling penguin under the words Happy Feet" } },
            { BuddyIconType.C2Muse, new[] { "Muse", "figures at a table in a red desert under a blue sky" } },
            { BuddyIconType.C2Snakes, new[] { "Snakes on a Plane", "the words Snakes on a Plane in orange on black" } },
            { BuddyIconType.C2LupeFiasco, new[] { "Lupe Fiasco", "a skateboarder flying through streaks of light" } },
            { BuddyIconType.C2Amy, new[] { "Amy Winehouse", "the name Amy Winehouse over a woman lying down" } },
            { BuddyIconType.C2ChemicalRomance, new[] { "My Chemical Romance", "a marching figure and the words Black Parade" } },
            { BuddyIconType.C2Tool, new[] { "Tool", "a red face covered in patterns" } },
            { BuddyIconType.C2Departed, new[] { "The Departed", "the words The Departed over grey skyscrapers" } },
            { BuddyIconType.C2CasinoRoyale, new[] { "Casino Royale", "Casino Royale and the 007 gun logo in gold" } },
            { BuddyIconType.C2TheWire, new[] { "The Wire", "the words The Wire over a blue sound wave" } },
            { BuddyIconType.C2Scrubs, new[] { "Scrubs", "the Scrubs logo over a blue chest X-ray" } },
            { BuddyIconType.C2CoH, new[] { "Company of Heroes", "a soldier in a helmet over the letters CoH" } },
            { BuddyIconType.C2Gears, new[] { "Gears of War", "the red skull-in-a-cog logo on black" } },
            { BuddyIconType.C2LolCat, new[] { "I Can Has Cheezburger", "a grey cat and the words Cheez Burger" } },
            { BuddyIconType.C2SpaghettiMonster, new[] { "Flying Spaghetti Monster", "a ball of noodles with eyes on stalks, over the letters FSM" } },

            // Chapter 3, January 2007
            { BuddyIconType.C3Manchester, new[] { "Manchester Orchestra", "a faded photo of a person in a bedroom" } },
            { BuddyIconType.C3Jumpsuit, new[] { "The Red Jumpsuit Apparatus", "the band's name in red and white on black" } },
            { BuddyIconType.C3Hannah, new[] { "Hannah Montana", "the Hannah Montana logo in green and white" } },
            { BuddyIconType.C3Slayer, new[] { "Slayer", "the jagged red Slayer logo and an upside-down cross" } },
            { BuddyIconType.C3LCD, new[] { "LCD Soundsystem", "the letters LCD beside a speaker" } },
            { BuddyIconType.C3Shins, new[] { "The Shins", "the word Shins over a black and cream pattern" } },
            { BuddyIconType.C3Beyonce, new[] { "Beyoncé", "a woman with curly hair and the word B'Day" } },
            { BuddyIconType.C3Mika, new[] { "Mika", "the name Mika over a rainbow of colours" } },
            { BuddyIconType.C3Avril, new[] { "Avril Lavigne", "the name Avril on a pink heart with crossbones" } },
            { BuddyIconType.C3Fallout, new[] { "Fall Out Boy", "Fall Out Boy in white script on a starry night" } },
            { BuddyIconType.C3Juno, new[] { "Juno", "two teenagers on orange stripes and the word Juno" } },
            { BuddyIconType.C3Children, new[] { "Children of Men", "a man's face beside the words Children of Men" } },
            { BuddyIconType.C3Rock, new[] { "30 Rock", "the words 30 Rock over the rainbow NBC peacock" } },
            { BuddyIconType.C3Lost, new[] { "Lost", "the octagonal Dharma Initiative logo" } },
            { BuddyIconType.C3GuitarHero, new[] { "Guitar Hero", "the Guitar Hero logo on red" } },
            { BuddyIconType.C3WoW, new[] { "World of Warcraft", "a night elf with green hair and glowing eyes" } },
            { BuddyIconType.C3Charlie, new[] { "Charlie the Unicorn", "a grey unicorn on a green hill" } },
            { BuddyIconType.C3Shoop, new[] { "Shoop da Whoop", "a red and white creature firing a laser from its mouth" } },

            // Chapter 4, April 2007
            { BuddyIconType.C4Arctic, new[] { "Arctic Monkeys", "the words Arctic Monkeys in yellow over a dark room" } },
            { BuddyIconType.C4NIN, new[] { "Nine Inch Nails", "the NIN logo over a figure in blue light" } },
            { BuddyIconType.C4Korn, new[] { "Korn", "the Korn logo over a brown creature" } },
            { BuddyIconType.C4Carrie, new[] { "Carrie Underwood", "a blonde woman beside the name Carrie" } },
            { BuddyIconType.C4Justice, new[] { "Justice", "a golden cross outline on black" } },
            { BuddyIconType.C4National, new[] { "The National", "The National in yellow over a white light" } },
            { BuddyIconType.C4ArcadeFire, new[] { "Arcade Fire", "a red ornate symbol inside a frame" } },
            { BuddyIconType.C4LinkinPark, new[] { "Linkin Park", "Linkin Park in black over a skyline" } },
            { BuddyIconType.C4Rihanna, new[] { "Rihanna", "the name Rihanna in red on teal" } },
            { BuddyIconType.C4SouljaBoy, new[] { "Soulja Boy", "a man in white shutter shades over the name Soulja Boy" } },
            { BuddyIconType.C4Zodiac, new[] { "Zodiac", "the word Zodiac with its crossed-circle symbol" } },
            { BuddyIconType.C4300, new[] { "300", "the number 300 on a brown and white battle scene" } },
            { BuddyIconType.C4Boondocks, new[] { "The Boondocks", "a boy with an afro under the word Boondocks" } },
            { BuddyIconType.C4Galactica, new[] { "Battlestar Galactica", "the word Galactica over a silver emblem" } },
            { BuddyIconType.C4Stalker, new[] { "S.T.A.L.K.E.R.", "a yellow radiation symbol over the word Stalker" } },
            { BuddyIconType.C4GodOfWar, new[] { "God of War", "the bronze omega symbol" } },
            { BuddyIconType.C4Chocolate, new[] { "Chocolate Rain", "a young man in glasses and the words Chocolate Rain" } },
            { BuddyIconType.C4Dramatic, new[] { "Dramatic Chipmunk", "a prairie dog turning to stare" } },

            // Chapter 5, June 2007
            { BuddyIconType.C5TheUsed, new[] { "The Used", "a pale creature's face under the words The Used" } },
            { BuddyIconType.C5Kings, new[] { "Kings of Leon", "Kings of Leon in white on black" } },
            { BuddyIconType.C5WhiteStripes, new[] { "The White Stripes", "the words Icky Thump in red around a face" } },
            { BuddyIconType.C5Delilah, new[] { "Plain White T's", "the words Hey There Delilah in blue" } },
            { BuddyIconType.C5Simpsons, new[] { "The Simpsons", "Homer Simpson's eyes and mouth on yellow" } },
            { BuddyIconType.C5Megadeth, new[] { "Megadeth", "the word Megadeth over rows of soldiers" } },
            { BuddyIconType.C5ModestMouse, new[] { "Modest Mouse", "an orange hot-air balloon shape on dark blue" } },
            { BuddyIconType.C5Kanye, new[] { "Kanye West", "a cartoon bear flying through a pink sky" } },
            { BuddyIconType.C5FooFighters, new[] { "Foo Fighters", "a grey object hanging on a cream background" } },
            { BuddyIconType.C5MIA, new[] { "M.I.A.", "the letters M.I.A. over bright triangles" } },
            { BuddyIconType.C5TheOffice, new[] { "The Office", "the word Office over two people at desks" } },
            { BuddyIconType.C5Greys, new[] { "Grey's Anatomy", "the word Grey's over the Seattle skyline" } },
            { BuddyIconType.C5Superbad, new[] { "Superbad", "McLovin's fake ID card" } },
            { BuddyIconType.C5Transformers, new[] { "Transformers", "the grey Autobot face logo" } },
            { BuddyIconType.C5Bioshock, new[] { "BioShock", "an art deco city under the sea" } },
            { BuddyIconType.C5TeamFortress, new[] { "Team Fortress 2", "the orange Team Fortress 2 logo" } },
            { BuddyIconType.C5Portal, new[] { "Portal", "the grey Companion Cube with a pink heart" } },
            { BuddyIconType.C5AdviceDog, new[] { "Advice Dog", "a grinning dog on a rainbow wheel" } },

            // Story icons: the icons Emily, Evelyn and Emerly used.
            { BuddyIconType.S0Emily1, new[] { "Emily's icon, Chapter 1", "a light blue snowflake on black" } },
            { BuddyIconType.S0Emily2, new[] { "Emily's icon, Chapter 2", "scratchy black symbols on cream" } },
            { BuddyIconType.S0Emily3, new[] { "Emily's icon, Chapter 3", "a faded photo of a person in a bedroom" } },
            { BuddyIconType.S0Emily4, new[] { "Emily's icon, Chapter 4", "the words Arctic Monkeys in yellow over a dark room" } },
            { BuddyIconType.S0Eva1, new[] { "Evelyn's icon, Chapter 1", "two men shaking hands, one of them on fire" } },
            { BuddyIconType.S0Eva2, new[] { "Evelyn's icon, Chapter 2", "a dark face with a red bar across the eyes" } },
            { BuddyIconType.S0Eva3, new[] { "Evelyn's icon, Chapter 3", "The Red Jumpsuit Apparatus in red and white on black" } },
            { BuddyIconType.S0Eva4, new[] { "Evelyn's icon, Chapter 4", "the NIN logo over a figure in blue light" } },
            { BuddyIconType.S0Emerly1, new[] { "Emerly's icon 1", "the word Coldplay on a glowing orange circle" } },
            { BuddyIconType.S0Emerly2, new[] { "Emerly's icon 2", "a glowing orange shape with small dark marks" } },
            { BuddyIconType.S0Emerly3, new[] { "Emerly's icon 3", "the word Muse over a blue pattern" } },
            { BuddyIconType.S0Emerly4, new[] { "Emerly's icon 4", "the word Plans under hanging lights" } },

            // Secret icons, page 1: games
            { BuddyIconType.S1Undertale, new[] { "Undertale", "Flowey's face in white on black" } },
            { BuddyIconType.S1Minecraft, new[] { "Minecraft", "Steve's blocky face" } },
            { BuddyIconType.S1FNAF, new[] { "Five Nights at Freddy's", "a dark animatronic face over the letters FNAF" } },
            { BuddyIconType.S1Overwatch, new[] { "Overwatch", "the Overwatch logo" } },
            { BuddyIconType.S1Inside, new[] { "Inside", "the word Inside in red over a small boy" } },
            { BuddyIconType.S1Firewatch, new[] { "Firewatch", "a pine tree on an orange badge" } },
            { BuddyIconType.S1NITW, new[] { "Night in the Woods", "Mae's blue cat face" } },
            { BuddyIconType.S1Vallhalla, new[] { "VA-11 Hall-A", "the VA-11 Hall-A logo in pink" } },
            { BuddyIconType.S1Hypnospace, new[] { "Hypnospace Outlaw", "the word Hypnospace on lilac" } },
            { BuddyIconType.S1Tacoma, new[] { "Tacoma", "the word Tacoma under a moon" } },
            { BuddyIconType.S1MakeSail, new[] { "Make Sail", "the words Make Sail over the sea" } },
            { BuddyIconType.S1RedLie, new[] { "Little Red Lie", "the words Little Red Lie under a dark red moon" } },

            // Secret icons, page 2: YouTubers
            { BuddyIconType.S2JackSepticEye, new[] { "Jacksepticeye", "a big green eye" } },
            { BuddyIconType.S2PewDiePie, new[] { "PewDiePie", "the blue brofist" } },
            { BuddyIconType.S2Markiplier, new[] { "Markiplier", "a man with black hair and a pink moustache" } },
            { BuddyIconType.S2KindaFunny, new[] { "Kinda Funny", "a smiling blue face" } },
            { BuddyIconType.S2h3h3, new[] { "h3h3", "the letters h3 in brackets on purple" } },
            { BuddyIconType.S2iDubbz, new[] { "iDubbbz", "a man pulling an angry face" } },
            { BuddyIconType.S2Dunkey, new[] { "videogamedunkey", "a grey donkey on pink" } },
            { BuddyIconType.S2ChilledChaos, new[] { "ChilledChaos", "a smiling boy in a red cap" } },
            { BuddyIconType.S2Grimecraft, new[] { "Grimecraft", "a man in headphones and goggles" } },
            { BuddyIconType.S2SSundee, new[] { "SSundee", "a blocky face with a brown beard" } },
            { BuddyIconType.S2Cellbit, new[] { "Cellbit", "a white C with an orange streak on black" } },
            { BuddyIconType.S2Squeezie, new[] { "Squeezie", "a blue squiggle on white" } },

            // Secret icons, page 3: shows and memes
            { BuddyIconType.S3RickMorty, new[] { "Rick and Morty", "Rick and Morty's faces" } },
            { BuddyIconType.S3SouthPark, new[] { "South Park", "the South Park sign over the four boys" } },
            { BuddyIconType.S3AquaTeen, new[] { "Aqua Teen Hunger Force", "the green and purple Mooninites" } },
            { BuddyIconType.S3AdventureTime, new[] { "Adventure Time", "Finn's face in his white hat" } },
            { BuddyIconType.S3Kappa, new[] { "Kappa", "the grey Kappa face" } },
            { BuddyIconType.S3NyanCat, new[] { "Nyan Cat", "a pop-tart cat in space" } },
            { BuddyIconType.S3Doge, new[] { "Doge", "a shiba dog's side-eye" } },
            { BuddyIconType.S3Kreygasm, new[] { "Kreygasm", "a face with eyes closed and mouth open" } },
            { BuddyIconType.S3Newgrounds, new[] { "Newgrounds", "the black and white tank on yellow" } },
            { BuddyIconType.S3TreeFriends, new[] { "Happy Tree Friends", "a pink animal with a bow and red eyes" } },
            { BuddyIconType.S3Homestar, new[] { "Homestar Runner", "Homestar's white face and red cap" } },
            { BuddyIconType.S3TopSecret, new[] { "Top secret", "a man with red hair in a dark coat" } },

            // Hidden icons
            { BuddyIconType.S4Developer, new[] { "The developer", "a bearded man in glasses" } },
            { BuddyIconType.S4Herobrine, new[] { "Herobrine", "a blocky face with glowing white eyes" } },
            { BuddyIconType.S4Hidden, new[] { "Soup", "a red and white soup can" } },
            { BuddyIconType.S4Hidden2, new[] { "Soup", "a teal soup can" } },
            { BuddyIconType.S4MakeSailDay, new[] { "Make Sail, day", "a white diamond emblem over the sea by day" } },
            { BuddyIconType.S4MakeSailNight, new[] { "Make Sail, night", "a white diamond emblem over the sea at night" } },
            { BuddyIconType.S4MakeSailRough, new[] { "Make Sail, storm", "a white diamond emblem over a red, stormy sea" } },
        };

        public static string Name(BuddyIconType t)
        {
            string[] v;
            return All.TryGetValue(t, out v) ? v[0] : t.ToString();
        }

        public static string Picture(BuddyIconType t)
        {
            string[] v;
            return All.TryGetValue(t, out v) ? v[1] : "";
        }

        // "Snow Patrol, a light blue snowflake on black"
        public static string Describe(BuddyIconType t)
        {
            string p = Picture(t);
            return p.Length > 0 ? Name(t) + ", " + p : Name(t);
        }
    }
}
