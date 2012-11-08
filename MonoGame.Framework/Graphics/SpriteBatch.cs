using System;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
	public class SpriteBatch : GraphicsResource
	{
	    readonly SpriteBatcher _batcher;

		SpriteSortMode _sortMode;
		BlendState _blendState;
		SamplerState _samplerState;
		DepthStencilState _depthStencilState; 
		RasterizerState _rasterizerState;		
		Effect _effect;
        bool _beginCalled;

		Effect _spriteEffect;

		Matrix _matrix;
		Rectangle _tempRect = new Rectangle (0,0,0,0);
		Vector2 _texCoordTL = new Vector2 (0,0);
		Vector2 _texCoordBR = new Vector2 (0,0);

		public SpriteBatch (GraphicsDevice graphicsDevice)
		{
			if (graphicsDevice == null) {
				throw new ArgumentException ("graphicsDevice");
			}	

			this.graphicsDevice = graphicsDevice;

            // Use a custom SpriteEffect so we can control the transformation matrix
            _spriteEffect = new Effect(graphicsDevice, SpriteEffect.Bytecode);

            _batcher = new SpriteBatcher(graphicsDevice);

            _beginCalled = false;
		}

		public void Begin ()
		{
            Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity);	
		}

		public void Begin (SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect, Matrix transformMatrix)
		{

			// defaults
			_sortMode = sortMode;
			_blendState = blendState ?? BlendState.AlphaBlend;
			_samplerState = samplerState ?? SamplerState.LinearClamp;
			_depthStencilState = depthStencilState ?? DepthStencilState.None;
			_rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;

			_effect = effect;
			
			_matrix = transformMatrix;

            // Setup things now so a user can chage them.
            if (sortMode == SpriteSortMode.Immediate)
				Setup();

            _beginCalled = true;
		}

		public void Begin (SpriteSortMode sortMode, BlendState blendState)
		{
			Begin (sortMode, blendState, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity);			
		}

		public void Begin (SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState)
		{
			Begin (sortMode, blendState, samplerState, depthStencilState, rasterizerState, null, Matrix.Identity);	
		}

		public void Begin (SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect)
		{
			Begin (sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, Matrix.Identity);			
		}

		public void End ()
		{	
			_beginCalled = false;

			if (_sortMode != SpriteSortMode.Immediate)
				Setup();

            _batcher.DrawBatch(_sortMode);
        }
		
		void Setup() 
        {
			graphicsDevice.BlendState = _blendState;
			graphicsDevice.DepthStencilState = _depthStencilState;
			graphicsDevice.RasterizerState = _rasterizerState;
			graphicsDevice.SamplerStates[0] = _samplerState;
			
            // Setup the default sprite effect.
			var vp = graphicsDevice.Viewport;
            var projection = Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 0, 1);

            // GL requires a half pixel offset where as DirectX and PSS does not.
#if PSS || DIRECTX
            var transform = _matrix * projection;
#else
			var halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);
			var transform = _matrix * (halfPixelOffset * projection);
#endif

			_spriteEffect.Parameters["MatrixTransform"].SetValue(transform);				                
			_spriteEffect.CurrentTechnique.Passes[0].Apply();

			// If the user supplied a custom effect then apply
            // it now to override the sprite effect.
            if (_effect != null)
			    _effect.CurrentTechnique.Passes[0].Apply();
		}
		
        void CheckValid(Texture2D texture)
        {
            if (texture == null)
                throw new ArgumentNullException("texture");
            if (!_beginCalled)
                throw new InvalidOperationException("Draw was called, but Begin has not yet been called. Begin must be called successfully before you can call Draw.");
        }

        void CheckValid(SpriteFont spriteFont, string text)
        {
            if (spriteFont == null)
                throw new ArgumentNullException("spriteFont");
            if (text == null)
                throw new ArgumentNullException("text");
            if (!_beginCalled)
                throw new InvalidOperationException("DrawString was called, but Begin has not yet been called. Begin must be called successfully before you can call DrawString.");
        }

        void CheckValid(SpriteFont spriteFont, StringBuilder text)
        {
            if (spriteFont == null)
                throw new ArgumentNullException("spriteFont");
            if (text == null)
                throw new ArgumentNullException("text");
            if (!_beginCalled)
                throw new InvalidOperationException("DrawString was called, but Begin has not yet been called. Begin must be called successfully before you can call DrawString.");
        }

		public void Draw (Texture2D texture,
				Vector2 position,
				Rectangle? sourceRectangle,
				Color color,
				float rotation,
				Vector2 origin,
				Vector2 scale,
				SpriteEffects effect,
				float depth)
		{
            CheckValid(texture);

            var w = texture.Width * scale.X;
            var h = texture.Height * scale.Y;
			if (sourceRectangle.HasValue)
            {
				w = sourceRectangle.Value.Width*scale.X;
				h = sourceRectangle.Value.Height*scale.Y;
			}

            DrawInternal(texture,
				new Vector4(position.X, position.Y, w, h),
				sourceRectangle,
				color,
				rotation,
				origin * scale,
				effect,
				depth);
		}

		public void Draw (Texture2D texture,
				Vector2 position,
				Rectangle? sourceRectangle,
				Color color,
				float rotation,
				Vector2 origin,
				float scale,
				SpriteEffects effect,
				float depth)
		{
            CheckValid(texture);

            var w = texture.Width * scale;
            var h = texture.Height * scale;
            if (sourceRectangle.HasValue)
            {
                w = sourceRectangle.Value.Width * scale;
                h = sourceRectangle.Value.Height * scale;
            }

            DrawInternal(texture,
                new Vector4(position.X, position.Y, w, h),
				sourceRectangle,
				color,
				rotation,
				origin * scale,
				effect,
				depth);
		}

		public void Draw (Texture2D texture,
			Rectangle destinationRectangle,
			Rectangle? sourceRectangle,
			Color color,
			float rotation,
			Vector2 origin,
			SpriteEffects effect,
			float depth)
		{
<<<<<<< HEAD
            CheckValid(texture);

            DrawInternal(texture,
			      new Vector4(destinationRectangle.X,
			                  destinationRectangle.Y,
			                  destinationRectangle.Width,
			                  destinationRectangle.Height),
			      sourceRectangle,
			      color,
			      rotation,
			      origin,
			      effect,
			      depth);
=======
			// Configure Display Orientation:
			if(lastDisplayOrientation != graphicsDevice.PresentationParameters.DisplayOrientation)
			{
				bool update = true;
				// check the display width and height make sure it matches the target orientation
				// before updating the matrix
				if (graphicsDevice.PresentationParameters.DisplayOrientation == DisplayOrientation.Portrait)
				{
				   if (graphicsDevice.DisplayMode.Width > graphicsDevice.DisplayMode.Height) update = false;	
				}
				else
				{
					if (graphicsDevice.DisplayMode.Width < graphicsDevice.DisplayMode.Height) update = false;
				}
				if (update)
				{
					// updates last display orientation (optimization)				
					lastDisplayOrientation = graphicsDevice.PresentationParameters.DisplayOrientation;
	
	                var deviceManager = (IGraphicsDeviceManager)Game.Instance.Services.GetService(typeof(IGraphicsDeviceManager));
	                if (deviceManager == null)
	                    return;
	
	                (deviceManager as GraphicsDeviceManager).ResetClientBounds();
	
					// make sure the viewport is correct
					this.graphicsDevice.SetViewPort(Game.Instance.Window.ClientBounds.Width, Game.Instance.Window.ClientBounds.Height);
						
					/*
					AndroidGameActivity.Game.Log("--------------- Start Change -----------");
					AndroidGameActivity.Game.Log(String.Format("DisplayMode = {0}", this.graphicsDevice.DisplayMode.ToString()));
					AndroidGameActivity.Game.Log(String.Format("Orientation = {0}", this.graphicsDevice.PresentationParameters.DisplayOrientation.ToString()));
					AndroidGameActivity.Game.Log(String.Format("ViewPort = {0}", this.graphicsDevice.Viewport.ToString()));
					AndroidGameActivity.Game.Log(String.Format("ViewScreen = {0}", matViewScreen.ToString()));
					AndroidGameActivity.Game.Log(String.Format("Projection = {0}", matProjection.ToString()));
					AndroidGameActivity.Game.Log(String.Format("ViewFramebuffer = {0}", matViewFramebuffer.ToString()));
					AndroidGameActivity.Game.Log("--------------- End Change -------------");
					*/
				}
			}
			
			if (!GraphicsDevice.DefaultFrameBuffer)
			{
				matProjection = Matrix.CreateOrthographic(this.graphicsDevice.Viewport.Width,
							this.graphicsDevice.Viewport.Height,
							-1f,1f);
				// we are using a render target so update
				matViewFramebuffer = Matrix.CreateTranslation(-this.graphicsDevice.Viewport.Width/2,
							-this.graphicsDevice.Viewport.Height/2,
							1);	
				
				matWVPFramebuffer = _matrix * matViewFramebuffer *  matProjection;
			}
			else
			{
					matViewScreen = Matrix.CreateRotationZ((float)Math.PI)*
								     	Matrix.CreateRotationY((float)Math.PI)*
										Matrix.CreateTranslation(-this.graphicsDevice.Viewport.Width/2,
										this.graphicsDevice.Viewport.Height/2,
										1);
					matProjection = Matrix.CreateOrthographic(this.graphicsDevice.Viewport.Width,
								this.graphicsDevice.Viewport.Height,
								-1f,1f);
#if !ANDROID
					if (graphicsDevice.PresentationParameters.DisplayOrientation == DisplayOrientation.LandscapeRight)
					{
						// flip the viewport	
						matProjection = Matrix.CreateOrthographic(-this.graphicsDevice.Viewport.Width,
								-this.graphicsDevice.Viewport.Height,
								-1f,1f);
					}
#endif
					matWVPScreen = _matrix * matViewScreen * matProjection;								    
			}
			
			
>>>>>>> origin
		}

		internal void DrawInternal (Texture2D texture,
			Vector4 destinationRectangle,
			Rectangle? sourceRectangle,
			Color color,
			float rotation,
			Vector2 origin,
			SpriteEffects effect,
			float depth)
		{
			var item = _batcher.CreateBatchItem();

			item.Depth = depth;
			item.Texture = texture;

			if (sourceRectangle.HasValue) {
				_tempRect = sourceRectangle.Value;
			} else {
				_tempRect.X = 0;
				_tempRect.Y = 0;
				_tempRect.Width = texture.Width;
				_tempRect.Height = texture.Height;				
			}
			
			_texCoordTL.X = _tempRect.X / (float)texture.Width;
			_texCoordTL.Y = _tempRect.Y / (float)texture.Height;
			_texCoordBR.X = (_tempRect.X + _tempRect.Width) / (float)texture.Width;
			_texCoordBR.Y = (_tempRect.Y + _tempRect.Height) / (float)texture.Height;

			if ((effect & SpriteEffects.FlipVertically) != 0) {
                var temp = _texCoordBR.Y;
				_texCoordBR.Y = _texCoordTL.Y;
				_texCoordTL.Y = temp;
			}
			if ((effect & SpriteEffects.FlipHorizontally) != 0) {
                var temp = _texCoordBR.X;
				_texCoordBR.X = _texCoordTL.X;
				_texCoordTL.X = temp;
			}

			item.Set (destinationRectangle.X,
					destinationRectangle.Y, 
					-origin.X, 
					-origin.Y, 
					destinationRectangle.Z,
					destinationRectangle.W,
					(float)Math.Sin (rotation), 
					(float)Math.Cos (rotation), 
					color, 
					_texCoordTL, 
					_texCoordBR);			
			
			if (_sortMode == SpriteSortMode.Immediate)
                _batcher.DrawBatch(_sortMode);
		}

		public void Draw (Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
		{
			Draw (texture, position, sourceRectangle, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}

		public void Draw (Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
		{
			Draw (texture, destinationRectangle, sourceRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0f);
		}

		public void Draw (Texture2D texture, Vector2 position, Color color)
		{
			Draw (texture, position, null, color);
		}

		public void Draw (Texture2D texture, Rectangle rectangle, Color color)
		{
			Draw (texture, rectangle, null, color);
		}

		public void DrawString (SpriteFont spriteFont, string text, Vector2 position, Color color)
		{
            CheckValid(spriteFont, text);

            var source = new SpriteFont.CharacterSource(text);
			spriteFont.DrawInto (
                this, ref source, position, color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
		}

		public void DrawString (
			SpriteFont spriteFont, string text, Vector2 position, Color color,
			float rotation, Vector2 origin, float scale, SpriteEffects effects, float depth)
		{
            CheckValid(spriteFont, text);

			var scaleVec = new Vector2(scale, scale);
            var source = new SpriteFont.CharacterSource(text);
            spriteFont.DrawInto(this, ref source, position, color, rotation, origin, scaleVec, effects, depth);
		}

		public void DrawString (
			SpriteFont spriteFont, string text, Vector2 position, Color color,
			float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
		{
            CheckValid(spriteFont, text);

            var source = new SpriteFont.CharacterSource(text);
            spriteFont.DrawInto(this, ref source, position, color, rotation, origin, scale, effect, depth);
		}

		public void DrawString (SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color)
		{
            CheckValid(spriteFont, text);

            var source = new SpriteFont.CharacterSource(text);
			spriteFont.DrawInto(this, ref source, position, color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
		}

		public void DrawString (
			SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color,
			float rotation, Vector2 origin, float scale, SpriteEffects effects, float depth)
		{
            CheckValid(spriteFont, text);

			var scaleVec = new Vector2 (scale, scale);
            var source = new SpriteFont.CharacterSource(text);
            spriteFont.DrawInto(this, ref source, position, color, rotation, origin, scaleVec, effects, depth);
		}

		public void DrawString (
			SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color,
			float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
		{
            CheckValid(spriteFont, text);

            var source = new SpriteFont.CharacterSource(text);
            spriteFont.DrawInto(this, ref source, position, color, rotation, origin, scale, effect, depth);
		}

        private bool _isDisposed;

        public override void Dispose()
        {
            if (_isDisposed)
                return;

            _spriteEffect.Dispose();
            _spriteEffect = null;

            _isDisposed = true;

            base.Dispose();
        }
	}
}

