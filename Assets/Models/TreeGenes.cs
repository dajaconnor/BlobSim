namespace Assets
{
    public class TreeGenes
    {
        public float growDropRatio;
        public int lifespan;
        public int fastGrowTime;
        public int mediumGrowTime;
        public int slowGrowTime;
        public float fiber;

        public TreeGenes(float growDropRatio, int lifespan, int fastGrowTime, int mediumGrowTime, int slowGrowTime, float fiber)
        {
            this.growDropRatio = growDropRatio;
            this.lifespan = lifespan;
            this.fastGrowTime = fastGrowTime;
            this.mediumGrowTime = mediumGrowTime;
            this.slowGrowTime = slowGrowTime;
            this.fiber = fiber;
        }
    }
}
