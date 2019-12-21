using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoShared {

	public enum GOFeatureType  {
		Line,
		MultiLine,
		Polygon,
		MultiPolygon,
		Point,
		MultiPoint,
		Label,
		Undefined
	}
	
	public enum GOFeatureKind {

		//Unknown
		baseKind,

		//Buildings
		abandoned,
		administrative,
		agricultural,
		airport,
		allotment_house,
		apartments,
		arbour,
		bank,
		barn,
		basilica,
		beach_hut,
		bell_tower,
		boathouse,
		brewery,
		bridge,
		bungalow,
		bunker,
		building_part,
		cabin,
		carport,
		castle,
		cathedral,
		chapel,
		chimney,
		church,
		civic,
		clinic,
		closed,
		clubhouse,
		collapsed,
		college,
		commercial,
		construction,
		container,
		convent,
		cowshed,
		dam,
		damaged,
		depot,
		destroyed,
		detached,
		disused,
		dormitory,
		duplex,
		factory,
		farm,
		farm_auxiliary,
		fire_station,
		garage,
		garages,
		gazebo,
		ger,
		glasshouse,
		government,
		grandstand,
		greenhouse,
		hangar,
		healthcare,
		hermitage,
		historical,
		hospital,
		hotel,
		house,
		houseboat,
		hut,
		industrial,
		kindergarten,
		kiosk,
		library,
		mall,
		manor,
		manufacture,
		mixed_use,
		mobile_home,
		monastery,
		mortuary,
		mosque,
		museum,
		office,
		outbuilding,
		parking,
		pavilion,
		power,
		prison,
		proposed,
		pub,
		residential,
		restaurant,
		retail,
		roof,
		ruin,
		ruins,
		school,
		semidetached_house,
		service,
		shed,
		shelter,
		shop,
		shrine,
		silo,
		slurry_tank,
		stable,
		stadium,
		static_caravan,
		storage,
		storage_tank,
		store,
		substation,
		summer_cottage,
		summer_house,
		supermarket,
		synagogue,
		tank,
		temple,
		terrace,
		tower,
		train_station,
		transformer_tower,
		transportation,
		university,
		utility,
		veranda,
		warehouse,
		wayside_shrine,
		works,


		//Landuse

		aerodrome,
		allotments,
		amusement_ride,
		animal,
		apron,
		aquarium,
		artwork,
		attraction,
		aviary,
		battlefield,
		beach,
		breakwater,
		camp_site,
		caravan_site,
		carousel,
		cemetery,
		cinema,
		city_wall,
		common,
		cutline,
		dike,
		dog_park,
		enclosure,
		farmland,
		farmyard,
		fence,
		footway,
		forest,
		fort,
		fuel,
		garden,
		gate,
		generator,
		glacier,
		golf_course,
		grass,
		grave_yard,
		groyne,
		hanami,
		land,
		maze,
		meadow,
		military,
		national_park,
		nature_reserve,
		natural_forest,// - See planned bug fix in #1096.
		natural_park,// - See planned bug fix in #1096.
		natural_wood,// - See planned bug fix in #1096.
		park,
		pedestrian,
		petting_zoo,
		picnic_site,
		pier,
		pitch,
		place_of_worship,
		plant,
		playground,
		protected_area,
		quarry,
		railway,
		recreation_ground,
		recreation_track,
		resort,
		rest_area,
		retaining_wall,
		rock,
		roller_coaster,
		runway,
		rural,
		scree,
		scrub,
		service_area,
		snow_fence,
		sports_centre,
		stone,
		summer_toboggan,
		taxiway,
		theatre,
		theme_park,
		trail_riding_station,
		tree_row,
		urban_area,
		urban,
		village_green,
		wastewater_plant,
		water_park,
		water_slide,
		water_works,
		wetland,
		wilderness_hut,
		wildlife_park,
		winery,
		winter_sports,
		wood,
		zoo,

		//Water

		basin, //polygon
		bay, //point, intended for label placement only
		canal, //line
		ditch, //line
		dock,  //polygon
		drain, //line
		fjord, //point, intended for label placement only
		lake, //polygon
		ocean, //polygon + point, intended for label placement only
		playa, //polygon
		river, //line
		riverbank, //polygon
		sea, //point, intended for label placement only
		stream, //line
		strait, //point, intended for label placement only
		swimming_pool, //polygon
		water, //polygon

		//Roads

		highway,
		major_road,
		minor_road,
		path,
		rail,
		ferry,
        tunnel

	};

	public enum GOLABELSKind {
		baseKind
	}

	public enum GOPOIKind {


		accountant,
		adit,
		administrative,
		advertising_agency,
		aerodrome,
		aeroway_gate,
		airport,
		alcohol,
		alpine_hut,
		ambulatory_care,
		amusement_ride,
		animal,
		aquarium,
		archaeological_site,
		architect,
		artwork,
		assisted_living,
		association,
		atm,
		attraction,
		aviary,
		bakery,
		bank,
		bar,
		battlefield,
		bbq,
		beach_resort,
		beach,
		beacon,
		bed_and_breakfast,
		bench,
		bicycle_parking,
		bicycle_rental,
		bicycle_rental_station,
		bicycle_repair_station,
		bicycle,
		bicycle_junction,
			block,
			boat_rental,
			boat_storage,
			bollard,
			books,
			brewery,
			bus_station,
			bus_stop,
			butcher,
			cafe,
			camp_site,
			car_repair,
			car_sharing,
			car,
			caravan_site,
			care_home,
			carousel,
			carpenter,
			cave_entrance,
			cemetery,
			chalet,
			childcare,
			childrens_centre,
			cinema,
			clinic,
			closed,
			clothes,
			club,
			college,
			communications_tower,
			community_centre,
			company,
			computer,
			confectionery,
			consulting,
			convenience,
			courthouse,
			cross,
			cycle_barrier,
				dairy_kitchen,
				dam,
				day_care,
				dentist,
				department_store,
				dive_centre,
				doctors,
				dog_park,
				doityourself,
				dressmaker,
				drinking_water,
				dry_cleaning,
				dune,
				educational_institution,
				egress,
				electrician,
				electronics,
				embassy,
				emergency_phone,
				employment_agency,
				enclosure,
				estate_agent,
				fashion,
				fast_food,
				farm,
				ferry_terminal,
				financial,
				fire_station,
				firepit,
				fishing,
				fishing_area,
				fitness_station,
				fitness,
				florist,
				food_bank,
				ford,
				forest,
				fort,
				foundation,
				fuel,
					gallery,
					garden,
					gardener,
					gas_canister,
						gate,
						generator,
						geyser,
						gift,
						golf_course,
						government,
						greengrocer,
						group_home,
						guest_house,
						hairdresser,
						halt,
						hanami,
						handicraft,
						hardware,
						hazard,
						healthcare,
						helipad,
						historical,
						hospital,
						hostel,
						hot_spring,
						hotel,
						hunting,
						hvac,
						ice_cream,
						information,
						insurance,
						it,
						jewelry,
						kindergarten,
						landmark,
						laundry,
						lawyer,
						level_crossing,
						library,
						life_ring,
						lifeguard_tower,
						lift_gate,
						lighthouse,
						
							mall,
							marina,
							mast,
							maze,
							memorial,
							metal_construction,
							midwife,
							military,
							mineshaft,
							mini_roundabout,
							mobile_phone,
							monument,
							motel,
							motorcycle,
							motorway_junction,
							museum,
							music,
							national_park,
							nature_reserve,
							newspaper,
							ngo,
							notary,
							nursing_home,
							observatory,
							offshore_platform,
							optician,
							outdoor,
							outreach,
							painter,
							park,
							parking,
							peak,
								pet,
								petroleum_well,
								petting_zoo,
								pharmacy,
								phone,
								photographer,
								photographic_laboratory,
								physician,
								picnic_site,
								picnic_table,
								pitch,
								place_of_worship,
								plant,
								playground ,
								plumber,
								police,
								political_party,
								post_box,
								post_office,
								pottery,
								power_pole,
								power_tower,
								power_wind,
								prison,
								protected_area,
								pub,
								put_in_egress,
								put_in,
								pylon,
								quarry,
								ranger_station,
								rapid,
								recreation_ground,
								recreation_track,
								recycling,
								refugee_camp,
								religion,
								research,
								residential_home,
								resort,
								rest_area,
								restaurant,
								rock,
								roller_coaster,
								saddle,
								sawmill,
								school,
								scuba_diving,
								service_area,
								shelter,
								shoemaker,
								shower,
								sinkhole,
								ski_rental,
								ski_school,
								ski,
								slipway,
								snow_cannon,
								social_facility,
								soup_kitchen,
								sports_centre,
								sports,
								spring,
								stadium,
								station,
								stone,
								stonemason,
								substation,
								subway_entrance,
								summer_camp,
								summer_toboggan,
								supermarket,
								swimming_area,
								tailor,
								tax_advisor,
								telecommunication,
								telephone,
								telescope,
								theatre,
								theme_park,
								therapist,
								toilets,
								toll_booth,
								townhall,
								toys,
								trade,
								traffic_signals,
								trail_riding_station,
								trailhead,
								tram_stop,
								travel_agent,
								tree,
								university,
								veterinary,
								viewpoint,
								volcano,
									walking_junction,
										waste_basket,
										waste_disposal,
										wastewater_plant,
										water_park,
										water_point,
										water_slide,
										water_tower,
										water_well,
										water_works,
										waterfall,
										watering_place,
										wilderness_hut,
										wildlife_park,
										windmill,
										wine,
										winery,
										winter_sports,
										wood,
										works,
										workshop,
										zoo,
		yes,
		UNDEFINED

	};

	public enum GOElevationAPI  {

		Mapzen,
		Mapbox
	}

	public class GOEnumUtils {

		#region LANDUSE

		public static GOFeatureKind MapzenToKind(string kind) {

			try {
				GOFeatureKind parsed_enum = (GOFeatureKind)System.Enum.Parse( typeof( GOFeatureKind ), kind );
				return parsed_enum;
			} catch {
				return GOFeatureKind.baseKind;
			}

		}

		public static GOFeatureKind MapboxToKind(string kind) {

//			Debug.Log (kind);

			if (kind == null)
				return GOFeatureKind.baseKind;

			//This is very empiric. looking forward to a more complete system

			if (kind.Contains ("motorway")) {
				return GOFeatureKind.highway;
			} else if (kind == "service" || kind == "secondary" || kind == "street" || kind == "tertiary" || kind == "transit" || kind == "minor") {
				return GOFeatureKind.minor_road;
			} else if (kind == "rail" || kind.Contains ("rail")) {
				return GOFeatureKind.rail;
			} else if (kind == "primary") {
				return GOFeatureKind.major_road;
			} else if (kind == "track") {
				return GOFeatureKind.path;
			} else if (kind == "sand") {
				return GOFeatureKind.beach;
			} 


			try {
				GOFeatureKind parsed_enum = (GOFeatureKind)System.Enum.Parse( typeof( GOFeatureKind ), kind );
				return parsed_enum;
			} catch {
				return GOFeatureKind.baseKind;
			}

		}
		#endregion

		#region POIS

		public static GOPOIKind PoiKindToEnum(string kind) {

			try {
				GOPOIKind parsed_enum = (GOPOIKind)System.Enum.Parse( typeof( GOPOIKind ), kind.ToLower() );
				return parsed_enum;
			} catch {
				return GOPOIKind.UNDEFINED;
			}

		}




		#endregion
	}




}