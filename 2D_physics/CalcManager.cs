﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace _2D_physics
{
    class CalcManager
    {
        private List<Figure> figures;

        public CalcManager(List<Figure> figures)
        {
            this.figures = figures;
        }

        //衝突判定
        //移動前に実行され、図形情報を更新
        private void DecideCollision()
        {
            for (int i = 0; i < figures.Count; i++)
            {
                Figure figureF = figures[i];
                for (int j = i + 1; j < figures.Count; j++)
                {
                    Figure figureL = figures[j];

                    if(BroadDecision(figureF, figureL))
                    {
                        //figureFとfigureLでNarrowDecisionを行う
                        NarrowDecision(figureF, figureL);
                    }
                }
            }

        }

        //ナロー検出　衝突していたならば動作変更まで行う
        private void NarrowDecision(Figure figure1, Figure figure2)
        {
            List<Line> lines1 = figure1.Lines;
            List<Line> lines2 = figure2.Lines;

            for (int i = 0; i < lines1.Count; i++)
            {
                for (int j = 0; j < lines2.Count; j++)
                {
                    if (IsLineCross(lines1[i], lines2[j])
                        && IsLineCross(lines1[(i + 1) % lines1.Count], lines2[j]))
                            Collision(lines1[i].end, lines2[j], figure1, figure2);
                }
            }

            for (int i = 0; i < lines2.Count; i++)
            {
                for (int j = 0; j < lines1.Count; j++)
                {
                    if (IsLineCross(lines2[i], lines1[j])
                        && IsLineCross(lines2[(i + 1) % lines2.Count], lines1[j]))
                        Collision(lines2[i].end, lines1[j], figure2, figure1);
                }
            }

        }
        //線分が交差しているかの判定関数
        private bool IsLineCross(Line line1, Line line2)
        {
            var ta = (line2.start.X - line2.end.X) * (line1.start.Y - line2.start.Y) 
                + (line2.start.Y - line2.end.Y) * (line2.start.X - line1.start.X);
            var tb = (line2.start.X - line2.end.X) * (line1.end.Y - line2.start.Y) 
                + (line2.start.Y - line2.end.Y) * (line2.start.X - line1.end.X);
            var tc = (line1.start.X - line1.end.X) * (line2.start.Y - line1.start.Y) 
                + (line1.start.Y - line1.end.Y) * (line1.start.X - line2.start.X);
            var td = (line1.start.X - line1.end.X) * (line2.end.Y - line1.start.Y) 
                + (line1.start.Y - line1.end.Y) * (line1.start.X - line2.end.X);

            return tc * td < 0 && ta * tb < 0;
        }

        //めり込んだ頂点（figure1）と辺（figure2）から双方の図形の運動情報を更新
        private void Collision(PointF point, Line line, Figure figure1, Figure figure2)
        {
            PointF sink = GetNormalVector(point, line);

            ChangeMove(new Line(point, new PointF(point.X + sink.X, point.Y + sink.Y)), figure1);
            ChangeMove(new Line(new PointF(point.X + sink.X, point.Y + sink.Y), point), figure2);
        }
        //点から線への法線ベクトル
        private PointF GetNormalVector(PointF point, Line line)
        {
            var M = (point.X - line.start.X) * (line.end.X - line.start.X)
                + (point.Y - line.start.Y) * (line.end.Y - line.start.Y);
            var N = (point.X - line.end.X) * (line.start.X - line.end.X)
                + (point.Y - line.end.Y) * (line.start.Y - line.end.Y);
            return new PointF(
                (N * line.start.X + M * line.end.X) / (M + N) - point.X,
                (N * line.start.Y + M * line.end.Y) / (M + N) - point.Y
                );
        }
        //外力のベクトルで図形の運動情報を更新
        //powerのstartが作用点
        //とりあえずは慣性モーメントは無視
        private void ChangeMove(Line power, Figure figure)
        {
            PointF v1 = new PointF(power.end.X - power.start.X, power.end.Y - power.start.Y);
            PointF v2 = new PointF(figure.Center.X - power.start.X, figure.Center.Y - power.start.Y);
            double distance = Math.Abs(CrossProduct(v1, v2)) / VectorAbs(v1);

            figure.Vel += new SizeF(power.end.X - power.start.X, power.end.Y - power.start.Y);
            figure.Angv -= 0.01 * distance * VectorAbs(v1)
                * DecideRotate(new PointF(power.start.X - figure.Center.X, power.start.Y - figure.Center.Y),
                                        new PointF(power.end.X - figure.Center.X, power.end.Y - figure.Center.Y));
        }
        //ベクトルの絶対値
        private double VectorAbs(PointF vector)
        {
            return Math.Pow(Math.Pow(vector.X, 2) + Math.Pow(vector.Y, 2), 0.5);
        }
        //ベクトルの外積
        private double CrossProduct(PointF v1, PointF v2)
        {
            return v1.X * v2.Y - v1.Y * v2.X;
        }
        //ベクトルの回転方向
        private int DecideRotate(PointF v1, PointF v2)
        {
            double conc = CrossProduct(v1, v2);
            return (conc >= 0 ? 1 : -1);
        }


        //ブロード検出　true:衝突　false:非衝突
        private bool BroadDecision(Figure figure1, Figure figure2)
        {
            RectangleF Rect1 = GetOuterRect(figure1);
            RectangleF Rect2 = GetOuterRect(figure2);

            return Rect1.IntersectsWith(Rect2);
        }
        //与えられた図形に外接する四角形を返す
        private RectangleF GetOuterRect(Figure figure)
        {
            List<PointF> points = figure.Points;

            PointF min = new PointF(points[0].X, points[0].Y);
            PointF max = new PointF(points[0].X, points[0].Y);

            points.ForEach(point =>
            {
                if (min.X > point.X) min.X = point.X;
                if (min.Y > point.Y) min.Y = point.Y;
                if (max.X < point.X) max.X = point.X;
                if (max.Y < point.Y) min.Y = point.Y;
            });

            return new RectangleF(min, new SizeF(max.X - min.X, max.Y - min.Y));
        }

        //図形を次のフレームへ移動させる
        public void MoveFigures()
        {
            //衝突判定
            DecideCollision();

            //平行移動
            figures.ForEach(figure =>
                 figure.Center += new SizeF((float)figure.Vel.X, (float)figure.Vel.Y) );

            //回転
            this.RotateFigures();
        }

        //回転動作のみを切り出したメソッド
        //重心からの相対位置を回転させる
        private void RotateFigures()
        {
            figures.ForEach(figure =>
            {
                //各点に対して回転動作
                //ForEach使うとうまく更新されない（構造体は参照渡しにならない？）ので通常ループで暫定対応
                List<PointF> points = figure.RelatePoints;
                for (int i = 0; i < points.Count; i++)
                {
                    PointF myPoint = points[i];
                    myPoint.X = (float)(points[i].X * Math.Cos(figure.Angv)
                                        - points[i].Y * Math.Sin(figure.Angv));

                    myPoint.Y = (float)(points[i].X * Math.Sin(figure.Angv)
                                        + points[i].Y * Math.Cos(figure.Angv));
                    points[i] = myPoint;
                }

            });
        }

    }
}
