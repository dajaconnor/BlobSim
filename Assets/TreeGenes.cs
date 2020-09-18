namespace Assets
{
    public class TreeGenes
    {
        public float growDropRatio;
        public int lifespan;
        public int fastGrowTime;
        public int mediumGrowTime;
        public int slowGrowTime;

        public TreeGenes(float growDropRatio, int lifespan, int fastGrowTime, int mediumGrowTime, int slowGrowTime)
        {
            this.growDropRatio = growDropRatio;
            this.lifespan = lifespan;
            this.fastGrowTime = fastGrowTime;
            this.mediumGrowTime = mediumGrowTime;
            this.slowGrowTime = slowGrowTime;
        }
    }
}
