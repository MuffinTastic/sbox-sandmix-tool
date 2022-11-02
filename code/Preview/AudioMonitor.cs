using Sandbox;
using System;
using Tools;

namespace SandMix.Tool.Preview;

public class AudioMonitor : Widget
{
	private const int BarCount = 32;
	private float[] LeftChannel = new float[BarCount];
	private float[] RightChannel = new float[BarCount];

	private const float YellowThreshold = 0.66f;
	private const float RedThreshold = 0.82f;

	public AudioMonitor( Widget parent ) : base( parent )
	{
		SetSizeMode( SizeMode.Default, SizeMode.Ignore );

		Size = new Vector2( 300.0f, 150.0f );
		MinimumSize = Size;

		SetLayout( LayoutMode.TopToBottom );
	}

	private void DrawChannel( float[] channel, float yoffset, float ysize, Color bgcolor )
	{
		var borders = new Vector2( 5.0f, 2.0f );
		var region = new Rect( borders.x, borders.y + yoffset, Width - 2 * borders.x, ysize - 2 * borders.y );

		var barCount = channel.Length;
		float gap = 2.0f;
		float barWidth = (region.Width - (barCount - 1) * gap) / barCount;

		void DrawBar( Rect region, int i, float sample )
		{
			var fade = (1.0f - (float)(i + 1) / barCount) * 0.75f;
			var x = i * (barWidth + gap);

			Paint.SetBrush( Color.Green.Darken( fade ) );
			var greenBarHeight = region.Height * MathF.Min( sample, YellowThreshold ) + 2;
			var rect = new Rect( region.Left + x, region.Top + region.Height - greenBarHeight, barWidth + 1, greenBarHeight );
			Paint.DrawRect( rect );

			if ( sample > YellowThreshold )
			{
				Paint.SetBrush( Color.Yellow.Darken( fade ) );
				var yellowThresholdHeight = region.Height * YellowThreshold;
				var yellowBarHeight = region.Height * (MathF.Min( sample, RedThreshold ) - YellowThreshold) + 2;
				rect = new Rect( region.Left + x, region.Top + region.Height - yellowBarHeight - yellowThresholdHeight, barWidth + 1, yellowBarHeight );
				Paint.DrawRect( rect );
			}

			if ( sample > RedThreshold )
			{
				Paint.SetBrush( Color.Red.Darken( fade ) );
				var redThresholdHeight = region.Height * RedThreshold;
				var redBarHeight = region.Height * (sample - RedThreshold);
				rect = new Rect( region.Left + x, region.Top + region.Height - redBarHeight - redThresholdHeight, barWidth + 1, redBarHeight );
				Paint.DrawRect( rect );
			}
		}

		Paint.SetPenEmpty();

		for ( int i = 0; i < barCount; i++ )
		{
			DrawBar( region, i, channel[i] );
		}

		var bgpatch = new Rect( 0.0f, region.Top + region.Height, Width, Height - region.Top + region.Height );

		Paint.SetBrush( bgcolor );
		Paint.DrawRect( bgpatch );

		// space for 7
		var lineSpacing = 7;
		var lineGap = region.Height / (lineSpacing - 1);

		Paint.SetPen( Color.Gray );

		// only actually draw 5
		for ( int i = 1; i < 6; i++ )
		{
			Paint.DrawLine( new Vector2( 0.0f, region.Top + i * lineGap ), new Vector2( Width, region.Top + i * lineGap ) );
		}
	}

	protected override void OnPaint()
	{
		base.OnPaint();

		Paint.SetPenEmpty();

		var bgcolor = new Color( 0.1f, 0.1f, 0.1f );
		var bgregion = new Rect( 0.0f, 0.0f, Width, Height );

		Paint.SetBrush( bgcolor );
		Paint.DrawRect( bgregion );

		DrawChannel( LeftChannel, 0.0f, Height / 2.0f, bgcolor );
		DrawChannel( RightChannel, Height / 2.0f, Height / 2.0f, bgcolor );

		Paint.SetPen( Color.White );
		Paint.DrawLine( new Vector2( 0.0f, Height / 2.0f ), new Vector2( Width, Height / 2.0f ) );
	}

	protected void AddSamples( float left, float right )
	{
		for (int i = 1; i < BarCount; i++ )
		{
			LeftChannel[i - 1] = LeftChannel[i];
			RightChannel[i - 1] = RightChannel[i];
		}

		LeftChannel[BarCount - 1] = left;
		RightChannel[BarCount - 1] = right;

		Update();
	}

	RealTimeSince lastUpdate = 0.0f;

	[Event.Frame]
	protected void TestUpdate()
	{
		if ( lastUpdate > 0.05f )
		{
			AddSamples( Rand.Float(), Rand.Float() );
			lastUpdate = 0.0f;
		}
	}
}
