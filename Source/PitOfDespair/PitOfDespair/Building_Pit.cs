using RimWorld;
using Verse;
using System.Linq;
using System.Text;


namespace PitOfDespair
{
    public class Building_Pit: Building
    {


        public override Graphic Graphic
        {
            get
            {
                if (this.GetComp<CompPit>().buildingGod== "cthulhu")
                {

                    Graphic newgraphic = GraphicDatabase.Get(this.def.graphicData.graphicClass, "Things/Building/PD_PitOfDespairTentacled", this.def.graphicData.shaderType.Shader, this.def.graphicData.drawSize, this.DrawColor, this.DrawColorTwo);

                    return newgraphic;
                }
                else if (this.GetComp<CompPit>().buildingGod == "bast")
                {

                    Graphic newgraphic = GraphicDatabase.Get(this.def.graphicData.graphicClass, "Things/Building/PD_PitOfDespairCats", this.def.graphicData.shaderType.Shader, this.def.graphicData.drawSize, this.DrawColor, this.DrawColorTwo);

                    return newgraphic;
                }
                else if (this.GetComp<CompPit>().buildingGod == "none")
                {

                    Graphic newgraphic = GraphicDatabase.Get(this.def.graphicData.graphicClass, "Things/Building/PD_PitOfDespair", this.def.graphicData.shaderType.Shader, this.def.graphicData.drawSize, this.DrawColor, this.DrawColorTwo);

                    return newgraphic;
                }

                else return base.Graphic;


            }
        }
    }
}
