using System;
using PureHDF;
using VirtualMaze.Assets.Scripts.Utils;

public class EyelinkMatFile : AbstractHDF5File {
    public readonly double[,] trial_index;
    public readonly int[,] trial_codes;
    public readonly uint[,] timestamps;
    public readonly float[,] eyePos;

    public readonly double[] fixationStarts;
    public readonly double[] fixationEnds;
    private static readonly int FIXATION_START_COL = 0;
    private static readonly int FIXATION_END_COL = 1;

    public int Length { get => timestamps.Length; }

    public EyelinkMatFile(string _filename) : base(_filename) {
        /*using (HDF5Group data = HDFHelper.GetMyDataGroup(this)) {
            trial_index = HDFHelper.GetDataMatrix<double>(data, "trial_timestamps");
            eyePos = HDFHelper.GetDataMatrix<float>(data, "eye_pos");
            timestamps = HDFHelper.GetDataMatrix<uint>(data, "timestamps");
            trial_codes = HDFHelper.GetDataMatrix<int>(data, "trial_codes");

            double[,] fixationTimes = HDFHelper.GetDataMatrix<double>(data, "fix_times");
            fixationStarts = TwoDArrayUtils.GetColumn(fixationTimes,FIXATION_START_COL);
            fixationEnds = TwoDArrayUtils.GetColumn(fixationTimes,FIXATION_END_COL);
        }
        */ 

        var file = H5File.OpenRead(_filename);
        var group_data = file.Group("el").Group("data");

        trial_index = HDFHelperNew.ReadMatrixDouble(group_data, "trial_timestamps");
        Console.Write($"Sample data: ({trial_index[0,1]}, {trial_index[0,2]})");

        eyePos = HDFHelperNew.ReadMatrixFloat(group_data, "eye_pos");
        timestamps = HDFHelperNew.ReadMatrixUInt(group_data, "timestamps");
        trial_codes = HDFHelperNew.ReadMatrixInt(group_data, "trial_codes");
        double[,] fixationTimes = HDFHelperNew.ReadMatrixDouble(group_data, "fix_times");
        fixationStarts = TwoDArrayUtils.GetColumn(fixationTimes,FIXATION_START_COL);
        fixationEnds = TwoDArrayUtils.GetColumn(fixationTimes,FIXATION_END_COL);

        Console.Write("Finished loading eyedata!");
    }



}
